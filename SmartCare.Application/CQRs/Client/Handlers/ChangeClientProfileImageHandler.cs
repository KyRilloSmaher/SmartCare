using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Client.Commands;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Client.Handlers
{
    public class ChangeClientProfileImageHandler : IRequestHandler<ChangeClientProfileImageAsyncCommand, Response<string>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IClientRepository _clientRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IRateRepository _rateRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Client;

        #endregion
        public ChangeClientProfileImageHandler(
            IResponseHandler responseHandler,
            IBackgroundJobService backgroundJobService,
            IClientRepository clientRepository,
            IRedisCacheService redisCacheService,
            IRateRepository rateRepository,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _backgroundJobService = backgroundJobService;
            _clientRepository = clientRepository;
            _redisCacheService = redisCacheService;
            _rateRepository = rateRepository;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }


        public async Task<Response<string>> Handle(ChangeClientProfileImageAsyncCommand request, CancellationToken cancellationToken)
        {
            var UserId = request.UserId;
            var dto = request.dto;
            if (string.IsNullOrWhiteSpace(UserId))
                return _responseHandler.BadRequest<string>(SystemMessages.USER_NOT_FOUND);
            var client = await _clientRepository.GetByIdAsync(UserId, true);
            if (client == null)
                return _responseHandler.NotFound<string>(SystemMessages.USER_NOT_FOUND);
            // Delete old image 
            var oldImageUrl = client.ProfileImageUrl;
            var DeleteResult = await _imageUploaderService.DeleteImageByUrlAsync(oldImageUrl);
            if (!DeleteResult)
                return _responseHandler.Failed<string>(SystemMessages.FAILED);
            var uploadResult = await _imageUploaderService.UploadImageAsync(dto.ProfileImage, ImageFolder.UserProfiles);
            if (uploadResult.Error != null)
            {
                await _clientRepository.RollbackTransactionAsync();
                return _responseHandler.Failed<string>(SystemMessages.FILE_UPLOAD_FAILED);
            }
            client.ProfileImageUrl = uploadResult.Url.ToString();
            var updateResult = await _clientRepository.UpdateAsync(client);
            string cacheKey = $"client_id_{UserId}";
            await _redisCacheService.RemoveKeyAsync(cacheKey, tag);
            return _responseHandler.Success(updateResult.ProfileImageUrl);
        }
    }
}
