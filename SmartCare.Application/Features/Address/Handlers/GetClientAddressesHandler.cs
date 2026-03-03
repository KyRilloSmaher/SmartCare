using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Address.Extensions;
using SmartCare.Application.CQRs.Address.Queries;
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
    public class GetClientAddressesHandler : IRequestHandler<GetClientAddressesAsyncQuery, Response<IEnumerable<AddressResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Addresses;


        #endregion
        public GetClientAddressesHandler(IResponseHandler responseHandler, IRedisCacheService redisCacheService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }


        public async Task<Response<IEnumerable<AddressResponseDto>>> Handle(GetClientAddressesAsyncQuery request, CancellationToken cancellationToken)
        {
            var clientId = request.clientId;
            var client = await _unitOfWork.Clients.GetValidClientAsync(clientId);
            if (client == null)
                return await _responseHandler.ClientNotFoundAsync<IEnumerable<AddressResponseDto>>();

            string cacheKey = $"client_addresses_{client.Id}";

            try
            {
                var cachedAddresses = await _redisCacheService.GetDataAsync<IEnumerable<AddressResponseDto>>(cacheKey, tag);
                if (cachedAddresses != null)
                {
                    return _responseHandler.Success(cachedAddresses);
                }
            }
            catch (Exception)
            {
            }

            var addresses = await _unitOfWork.Addresses.GetClientAddressesAsync(client.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var responseDto = _mapper.Map<IEnumerable<AddressResponseDto>>(addresses);

            if (responseDto != null)
            {
                await _redisCacheService.SetDataAsync(cacheKey, responseDto, tag, TimeSpan.FromHours(1));
            }

            return _responseHandler.Success(responseDto);
        }
    }
}
