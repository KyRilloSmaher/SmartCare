using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.CQRs.Client.Commands;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Client.Handlers
{
    public class ChangeClientProfileImageHandler : IRequestHandler<ChangeClientProfileImageAsyncCommand, Response<string>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Client;
        #endregion

        public ChangeClientProfileImageHandler(
            IResponseHandler responseHandler,
            IBackgroundJobService backgroundJobService,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
            _backgroundJobService = backgroundJobService;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<string>> Handle(ChangeClientProfileImageAsyncCommand request, CancellationToken cancellationToken)
        {
            var userId = request.UserId;
            var dto = request.dto;

            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.BadRequest<string>(SystemMessages.USER_NOT_FOUND);

            // Fetch user via Identity
            var user = await _unitOfWork.UserManager.FindByIdAsync(userId);
            if (user == null)
                return _responseHandler.NotFound<string>(SystemMessages.USER_NOT_FOUND);

            // Delete old image
            var oldImageUrl = user.ProfileImageUrl;
            var deleteResult = await _imageUploaderService.DeleteImageByUrlAsync(oldImageUrl);
            if (!deleteResult)
                return _responseHandler.Failed<string>(SystemMessages.FAILED);

            // Upload new image
            var uploadResult = await _imageUploaderService.UploadImageAsync(dto.ProfileImage, ImageFolder.UserProfiles);
            if (uploadResult.Error != null)
                return _responseHandler.Failed<string>(SystemMessages.FILE_UPLOAD_FAILED);

            // Update user profile image
            user.ProfileImageUrl = uploadResult.Url.ToString();
            var updateResult = await _unitOfWork.UserManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return _responseHandler.Failed<string>(string.Join(", ", updateResult.Errors));

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Remove cache
            string cacheKey = $"client_id_{userId}";
            await _redisCacheService.RemoveKeyAsync(cacheKey, tag);

            return _responseHandler.Success(user.ProfileImageUrl);
        }
    }
}