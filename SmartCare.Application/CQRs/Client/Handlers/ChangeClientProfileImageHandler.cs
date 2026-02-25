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
        private readonly UserManager<ApplictionUser> _userManager;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IRateRepository _rateRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Client;
        #endregion

        public ChangeClientProfileImageHandler(
            IResponseHandler responseHandler,
            IBackgroundJobService backgroundJobService,
            UserManager<ApplictionUser> userManager,
            IRedisCacheService redisCacheService,
            IRateRepository rateRepository,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _backgroundJobService = backgroundJobService;
            _userManager = userManager;
            _redisCacheService = redisCacheService;
            _rateRepository = rateRepository;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }

        public async Task<Response<string>> Handle(ChangeClientProfileImageAsyncCommand request, CancellationToken cancellationToken)
        {
            var userId = request.UserId;
            var dto = request.dto;

            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.BadRequest<string>(SystemMessages.USER_NOT_FOUND);

            // Fetch user via Identity
            var user = await _userManager.FindByIdAsync(userId);
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
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return _responseHandler.Failed<string>(string.Join(", ", updateResult.Errors));

            // Remove cache
            string cacheKey = $"client_id_{userId}";
            await _redisCacheService.RemoveKeyAsync(cacheKey, tag);

            return _responseHandler.Success(user.ProfileImageUrl);
        }
    }
}