using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Address.Commands;
using SmartCare.Application.CQRs.Address.Extensions;
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
    public class DeleteClientAddressHandler : IRequestHandler<DeleteClientAddressAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Addresses;

        #endregion
        public DeleteClientAddressHandler(IResponseHandler responseHandler, IRedisCacheService redisCacheService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }


        public async Task<Response<bool>> Handle(DeleteClientAddressAsyncCommand request, CancellationToken cancellationToken)
        {
            var clientId = request.clientId;
            var addressId = request.addressId;
            var client = await _unitOfWork.Clients.GetValidClientAsync(clientId);
            if (client == null)
                return await _responseHandler.ClientNotFoundAsync<bool>();

            var address = await _unitOfWork.Addresses.GetByIdAsync(addressId, true);
            if (address == null || address.ClientId != client.Id)
                return _responseHandler.NotFound<bool>(SystemMessages.ADDRESS_NOT_FOUND);

            await _unitOfWork.Addresses.DeleteAsync(address);
            var remainingAddresses = await _unitOfWork.Addresses.GetClientAddressesAsync(client.Id);

            // If only one address left, ensure it becomes primary
            if (remainingAddresses.Count() == 1)
            {
                var lastAddress = remainingAddresses.First();

                if (!lastAddress.IsPrimary)
                {
                    lastAddress.IsPrimary = true;
                    
                }
            }

            await _unitOfWork.SaveChangesAsync();
            string cacheKey = $"client_addresses_{client.Id}";

            await _redisCacheService.RemoveKeyAsync(cacheKey, tag);

            return _responseHandler.Success(true);
        }
    }
}
