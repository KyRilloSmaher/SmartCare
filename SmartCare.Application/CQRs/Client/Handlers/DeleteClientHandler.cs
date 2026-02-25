using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.CQRs.Client.Commands;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Client.Handlers
{
    public class DeleteClientHandler : IRequestHandler<DeleteClientAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IClientRepository _clientRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IRateRepository _rateRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly UserManager<ApplictionUser> _userManager;
        private readonly IMapper _mapper;
        private const string tag = CacheConstants.Client;
        #endregion

        public DeleteClientHandler(
            IResponseHandler responseHandler,
            IBackgroundJobService backgroundJobService,
            IClientRepository clientRepository,
            IRedisCacheService redisCacheService,
            IRateRepository rateRepository,
            IImageUploaderService imageUploaderService,
            UserManager<ApplictionUser> userManager,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _backgroundJobService = backgroundJobService;
            _clientRepository = clientRepository;
            _redisCacheService = redisCacheService;
            _rateRepository = rateRepository;
            _imageUploaderService = imageUploaderService;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<Response<bool>> Handle(DeleteClientAsyncCommand request, CancellationToken cancellationToken)
        {
            var id = request.id;

            if (string.IsNullOrWhiteSpace(id))
                return _responseHandler.BadRequest<bool>(SystemMessages.USER_NOT_FOUND);

            try
            {
                await _clientRepository.BeginTransactionAsync();

                // Fetch the client domain entity
                var client = await _clientRepository.GetByIdAsync(id, true);
                if (client == null)
                    return _responseHandler.NotFound<bool>(SystemMessages.USER_NOT_FOUND);

                // Get profile image URL
                var imageUrl = client.User.ProfileImageUrl;

                // Soft delete client domain entity
                var deleteResult = await _clientRepository.DeleteAsync(client);

                if (!deleteResult)
                    throw new Exception("Failed to delete client domain entity.");

                // Soft delete Identity user
                var user = client.User;
                user.IsDeleted = true;
                var identityResult = await _userManager.UpdateAsync(user);
                if (!identityResult.Succeeded)
                    throw new Exception("Failed to mark Identity user as deleted.");

                // Delete profile image
                var deleteImageResult = await _imageUploaderService.DeleteImageByUrlAsync(imageUrl);
                if (!deleteImageResult)
                    throw new Exception("Failed to delete profile image.");

                await _clientRepository.CommitTransactionAsync();

                // Clear cache
                await _redisCacheService.RemoveKeyAsync($"client_id_{id}", tag);
                await _redisCacheService.RemoveKeyAsync("clients_all", tag);
                await _redisCacheService.RemoveKeyAsync($"client_email_{user.Email?.ToLower()}", tag);

                // Enqueue background job to mark rates as deleted
                _backgroundJobService.Enqueue(() => _rateRepository.MarkAllClientRatesAsDeleted(id));

                return _responseHandler.Success(true, SystemMessages.SUCCESS);
            }
            catch (Exception)
            {
                await _clientRepository.RollbackTransactionAsync();
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}