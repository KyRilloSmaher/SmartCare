using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Address.Commands;
using SmartCare.Application.CQRs.Address.Extensions;
using SmartCare.Application.DTOs.Address.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Address.Handlers
{
    public class AddNewClientAddressHandler : IRequestHandler<AddNewClientAddressAsyncCommand, Response<AddressResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IClientRepository _clientRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Addresses;
        #endregion


        public AddNewClientAddressHandler(
            IResponseHandler responseHandler,
            IClientRepository clientRepository,
            IAddressRepository addressRepository,
            IRedisCacheService redisCacheService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _clientRepository = clientRepository;
            _addressRepository = addressRepository;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
        }

        public async Task<Response<AddressResponseDto>> Handle(AddNewClientAddressAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            var clientId = request.clientId;
            var client = await _clientRepository.GetValidClientAsync(clientId);
            if (client == null)
                return await _responseHandler.ClientNotFoundAsync<AddressResponseDto>();

            var address = _mapper.Map<SmartCare.Domain.Entities.Address>(dto);
            address.ClientId = client.Id;

            if (dto.IsPrimary)
            {
                await _addressRepository.HandlePrimaryAddressChangeAsync(client.Id, address);
            }

            await _addressRepository.AddAsync(address);

            string cacheKey = $"client_addresses_{client.Id}";

            await _redisCacheService.RemoveKeyAsync(cacheKey, tag);
            var responseDto = _mapper.Map<AddressResponseDto>(address);
            return _responseHandler.Created(responseDto);
        }
    }
}
