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
    public class UpdateClientAddressHandler : IRequestHandler<UpdateClientAddressAsyncCommand, Response<AddressResponseDto>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Addresses;

        #endregion
        public UpdateClientAddressHandler(IResponseHandler responseHandler, IRedisCacheService redisCacheService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }


        public async Task<Response<AddressResponseDto>> Handle(UpdateClientAddressAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.dto;
            var clientId = request.clientId;
            var client = await _unitOfWork.Clients.GetValidClientAsync(clientId);
            if (client == null)
                return await _responseHandler.ClientNotFoundAsync<AddressResponseDto>();

            var address = await _unitOfWork.Addresses.GetByIdAsync(dto.Id, true);
            if (address == null || address.ClientId != client.Id)
                return _responseHandler.NotFound<AddressResponseDto>(SystemMessages.ADDRESS_NOT_FOUND);

            var primaryChange = dto.IsPrimary && !address.IsPrimary;

            if (primaryChange)
            {
                await _unitOfWork.Addresses.HandlePrimaryAddressChangeAsync(client.Id, address);
            }

            _mapper.Map(dto, address);
            address.IsPrimary = dto.IsPrimary;

            await _unitOfWork.SaveChangesAsync();

            string cacheKey = $"client_addresses_{client.Id}";
            string clientByIdKey = $"client_id_{clientId}";
            string clientByEmailKey = $"client_email_{client.User?.Email?.ToLower()}";
            await _redisCacheService.RemoveKeyAsync(clientByIdKey, CacheConstants.Client);
            await _redisCacheService.RemoveKeyAsync(clientByEmailKey, CacheConstants.Client);
            await _redisCacheService.RemoveKeyAsync("clients_all", CacheConstants.Client);
            // Remove cache for store
            await _redisCacheService.RemoveKeyAsync(cacheKey, tag);

            var responseDto = _mapper.Map<AddressResponseDto>(address);
            return _responseHandler.Success(responseDto);
        }
    }
}
