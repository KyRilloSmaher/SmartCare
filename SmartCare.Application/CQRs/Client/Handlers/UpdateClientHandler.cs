using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.CQRs.Client.Commands;
using SmartCare.Application.DTOs.Client.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Client.Handlers
{
    public class UpdateClientHandler : IRequestHandler<UpdateClientAsyncCommand, Response<ClientResponseDto?>>
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

        public UpdateClientHandler(
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

        public async Task<Response<ClientResponseDto?>> Handle(UpdateClientAsyncCommand request, CancellationToken cancellationToken)
        {
            var clientId = request.Id;
            var clientDto = request.ClientDto;

            if (string.IsNullOrWhiteSpace(clientId))
                return _responseHandler.BadRequest<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            // Fetch client domain entity including Identity user using UnitOfWork
            var client = await _unitOfWork.Clients.GetByIdAsync(clientId, true);
            if (client == null)
                return _responseHandler.NotFound<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            var oldEmail = client.User.Email;

            // Update Identity fields if changed
            if (!string.IsNullOrEmpty(clientDto.UserName) && clientDto.UserName != client.User.UserName)
            {
                var identityUser = client.User;
                identityUser.UserName = clientDto.UserName ?? identityUser.UserName;
                var identityUpdateResult = await _unitOfWork.UserManager.UpdateAsync(identityUser);
                if (!identityUpdateResult.Succeeded)
                    return _responseHandler.Failed<ClientResponseDto?>(
                        string.Join(", ", identityUpdateResult.Errors.Select(e => e.Description))
                    );
            }

            // Map other Client DTO fields
            _mapper.Map(clientDto, client);
            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Clear cache
            await _redisCacheService.RemoveKeyAsync($"client_id_{clientId}", tag);
            if (!string.IsNullOrEmpty(oldEmail))
                await _redisCacheService.RemoveKeyAsync($"client_email_{oldEmail.ToLower()}", tag);

            await _redisCacheService.RemoveKeyAsync("clients_all", tag);
            await _redisCacheService.DeleteKeysByTag(tag);

            // Map response DTO
            var updatedClientDto = _mapper.Map<ClientResponseDto?>(client);
            return _responseHandler.Success(updatedClientDto);
        }
    }
}