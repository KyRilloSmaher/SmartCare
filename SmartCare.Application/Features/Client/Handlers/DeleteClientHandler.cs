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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private const string tag = CacheConstants.Client;
        #endregion

        public DeleteClientHandler(
            IResponseHandler responseHandler,
            IBackgroundJobService backgroundJobService,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _backgroundJobService = backgroundJobService;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }

        public async Task<Response<bool>> Handle(DeleteClientAsyncCommand request, CancellationToken cancellationToken)
        {
            var id = request.id;

            if (string.IsNullOrWhiteSpace(id))
                return _responseHandler.BadRequest<bool>(SystemMessages.USER_NOT_FOUND);

            try
            {
                // Fetch the client domain entity using UnitOfWork
                var client = await _unitOfWork.Clients.GetByIdAsync(id, true);
                if (client == null)
                    return _responseHandler.NotFound<bool>(SystemMessages.USER_NOT_FOUND);

                // Get profile image URL
                var imageUrl = client.User.ProfileImageUrl;

                // Soft delete client domain entity
                await _unitOfWork.Clients.DeleteAsync(client);


                // Soft delete Identity user
                var user = client.User;
                user.IsDeleted = true;
                var identityResult = await _unitOfWork.UserManager.UpdateAsync(user);
                if (!identityResult.Succeeded)
                    throw new Exception("Failed to mark Identity user as deleted.");

                // Save all changes atomically through UnitOfWork
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Delete profile image (after successful database changes)
                var deleteImageResult = await _imageUploaderService.DeleteImageByUrlAsync(imageUrl);
                if (!deleteImageResult)
                {
                    // Log this but don't rollback transaction - image deletion failure shouldn't undo user deletion
                    // Consider adding logging here
                }

                // Clear cache
                await _redisCacheService.RemoveKeyAsync($"client_id_{id}", tag);
                await _redisCacheService.RemoveKeyAsync("clients_all", tag);
                await _redisCacheService.RemoveKeyAsync($"client_email_{user.Email?.ToLower()}", tag);

                // Enqueue background job to mark rates as deleted
                _backgroundJobService.Enqueue(() => _unitOfWork.Rates.MarkAllClientRatesAsDeletedAsync(id));

                return _responseHandler.Success(true, SystemMessages.SUCCESS);
            }
            catch (Exception ex)
            {
                // UnitOfWork will automatically rollback if SaveChangesAsync fails
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}