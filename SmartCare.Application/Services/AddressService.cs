using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SmartCare.Application.DTOs.Address.Requests;
using SmartCare.Application.DTOs.Address.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Interfaces.IServices;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Services
{

    public class AddressService : IAddressService
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IClientRepository _clientRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IMapper _mapper;
        #endregion
        #region Constructor
        public AddressService(
            IResponseHandler responseHandler,
            IClientRepository clientRepository,
            IMapper mapper,
            IAddressRepository addressRepository)
        {
            _responseHandler = responseHandler;
            _clientRepository = clientRepository;
            _mapper = mapper;
            _addressRepository = addressRepository;
        }
        #endregion

        #region Service Methods


        public async Task<Response<AddressResponseDto>> AddNewClientAddressAsync(string clientId, CreateAddressRequestDto dto)
        {
            var client = await GetValidClientAsync(clientId);
            if (client == null)
                return await ClientNotFound<AddressResponseDto>();

            var address = _mapper.Map<Address>(dto);
            address.ClientId = client.Id;

            if (dto.IsPrimary)
            {
                await HandlePrimaryAddressChangeAsync(client.Id, address);
            }

            await _addressRepository.AddAsync(address);

            var responseDto = _mapper.Map<AddressResponseDto>(address);
            return _responseHandler.Created(responseDto);
        }

        public async Task<Response<IEnumerable<AddressResponseDto>>> GetClientAddressesAsync(string clientId)
        {
            var client = await GetValidClientAsync(clientId);
            if (client == null)
                return await ClientNotFound<IEnumerable<AddressResponseDto>>();

            var addresses = await _addressRepository.GetClientAddressesAsync(client.Id);
            var responseDto = _mapper.Map<IEnumerable<AddressResponseDto>>(addresses);

            return _responseHandler.Success(responseDto);
        }

        public async Task<Response<bool>> DeleteClientAddressAsync(string clientId, Guid addressId)
        {
            var client = await GetValidClientAsync(clientId);
            if (client == null)
                return await ClientNotFound<bool>();

            var address = await _addressRepository.GetByIdAsync(addressId, true);
            if (address == null || address.ClientId != client.Id)
                return _responseHandler.NotFound<bool>(SystemMessages.ADDRESS_NOT_FOUND);

            await _addressRepository.DeleteAsync(address);
            var remainingAddresses = await _addressRepository.GetClientAddressesAsync(client.Id);

            // If only one address left, ensure it becomes primary
            if (remainingAddresses.Count() == 1)
            {
                var lastAddress = remainingAddresses.First();

                if (!lastAddress.IsPrimary)
                {
                    lastAddress.IsPrimary = true;
                    await _addressRepository.UpdateAsync(lastAddress);
                }
            }

            return _responseHandler.Success(true);
        }


        public async Task<Response<AddressResponseDto>> UpdateClientAddressAsync(string clientId, UpdateAddressRequestDto dto)
        {
            var client = await GetValidClientAsync(clientId);
            if (client == null)
                return await ClientNotFound<AddressResponseDto>();

            var address = await _addressRepository.GetByIdAsync(dto.Id, true);
            if (address == null || address.ClientId != client.Id)
                return _responseHandler.NotFound<AddressResponseDto>(SystemMessages.ADDRESS_NOT_FOUND);

            var primaryChange = dto.IsPrimary && !address.IsPrimary;

            if (primaryChange)
            {
                await HandlePrimaryAddressChangeAsync(client.Id, address);
            }

            _mapper.Map(dto, address);
            address.IsPrimary = dto.IsPrimary;

            await _addressRepository.UpdateAsync(address);

            var responseDto = _mapper.Map<AddressResponseDto>(address);
            return _responseHandler.Success(responseDto);
        }

        public async Task<Response<AddressResponseDto>> SetAddressAsPrimaryAddressAsync(string clientId, Guid addressId)
        {
            var client = await GetValidClientAsync(clientId);
            if (client == null)
                return await ClientNotFound<AddressResponseDto>();

            var address = await _addressRepository.GetByIdAsync(addressId, true);
            if (address == null || address.ClientId != client.Id)
                return _responseHandler.NotFound<AddressResponseDto>(SystemMessages.ADDRESS_NOT_FOUND);

            await HandlePrimaryAddressChangeAsync(client.Id, address);
            await _addressRepository.UpdateAsync(address);

            var responseDto = _mapper.Map<AddressResponseDto>(address);
            return _responseHandler.Success(responseDto);
        }

        #endregion
        #region Helpers

        private async Task<Client?> GetValidClientAsync(string clientId)
        {
            return await _clientRepository.GetByIdAsync(clientId);
        }

        private async Task<Response<T>> ClientNotFound<T>()
        {
            return _responseHandler.NotFound<T>(SystemMessages.USER_NOT_FOUND);
        }

        private async Task HandlePrimaryAddressChangeAsync(string clientId, Address newPrimary)
        {
            var currentPrimary = await _addressRepository.GetPrimaryAddressAsync(clientId);

            if (currentPrimary != null && currentPrimary.Id != newPrimary.Id)
            {
                currentPrimary.IsPrimary = false;
                await _addressRepository.UpdateAsync(currentPrimary);
            }

            newPrimary.IsPrimary = true;
        }
        #endregion
    }

}
