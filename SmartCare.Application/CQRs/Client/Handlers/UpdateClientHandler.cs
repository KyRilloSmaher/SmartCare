using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Client.Commands;
using SmartCare.Application.DTOs.Client.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Client.Handlers
{
    public class UpdateClientHandler : IRequestHandler<UpdateClientAsyncCommand, Response<ClientResponseDto?>>
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
        public UpdateClientHandler(
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


        public async Task<Response<ClientResponseDto?>> Handle(UpdateClientAsyncCommand request, CancellationToken cancellationToken)
        {
            var Id = request.Id;
            var ClientDto = request.ClientDto;
            if (string.IsNullOrWhiteSpace(Id))
                return _responseHandler.BadRequest<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);
            var client = await _clientRepository.GetByIdAsync(Id, true);
            if (client == null)
                return _responseHandler.NotFound<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);
            var oldEmail = client.Email;
            _mapper.Map(ClientDto, client);
            var updatedClient = await _clientRepository.UpdateAsync(client);

            var key = $"client_id_{Id}";
            await _redisCacheService.RemoveKeyAsync($"client_id_{Id}", tag);
            if (!string.IsNullOrEmpty(oldEmail))
                await _redisCacheService.RemoveKeyAsync($"client_email_{oldEmail.ToLower()}", tag);

            await _redisCacheService.RemoveKeyAsync("clients_all", tag);
            await _redisCacheService.DeleteKeysByTag(tag);

            var updatedClientDto = _mapper.Map<ClientResponseDto?>(updatedClient);
            return _responseHandler.Success(updatedClientDto);
        }
    }
}
