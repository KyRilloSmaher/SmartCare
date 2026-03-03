using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Inventory.Queries;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Inventory.Handlers
{
    public class GetBestInventoryIdHandler : IRequestHandler<GetBestInventoryIdQuery, Response<Guid>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public GetBestInventoryIdHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<Guid>> Handle(GetBestInventoryIdQuery request, CancellationToken cancellationToken)
        {
            var productId = request.productId;
            var quantityRequired = request.quantityRequired;
            
            if (productId == Guid.Empty || quantityRequired <= 0)
                return _responseHandler.BadRequest<Guid>(SystemMessages.INVALID_INPUT);
                
            var inventory = await _unitOfWork.Inventories.GetAvailableInventoryAsync(productId, quantityRequired);
            
            if (inventory is null)
                return _responseHandler.Failed<Guid>(SystemMessages.NOT_FOUND);
                
            return _responseHandler.Success(inventory.Id);
        }
    }
}