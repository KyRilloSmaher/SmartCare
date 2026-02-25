using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Inventory.Commands;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Inventory.Handlers
{
    public class DeleteInventoryHandler : IRequestHandler<DeleteInventoryAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public DeleteInventoryHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<bool>> Handle(DeleteInventoryAsyncCommand request, CancellationToken cancellationToken)
        {
            var Id = request.Id;

            if (Id == Guid.Empty)
                return _responseHandler.BadRequest<bool>(SystemMessages.INVALID_INPUT);

            var inventory = await _unitOfWork.Inventories.GetByIdAsync(Id);
            if (inventory == null)
                return _responseHandler.NotFound<bool>(SystemMessages.NOT_FOUND);

            await _unitOfWork.Inventories.DeleteAsync(inventory);

            // Save changes through UnitOfWork
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
        }
    }
}