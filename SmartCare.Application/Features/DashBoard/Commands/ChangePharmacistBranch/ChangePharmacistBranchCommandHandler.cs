using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Commands.ChangePharmacistBranch
{
    public class ChangePharmacistBranchCommandHandler
    : IRequestHandler<ChangePharmacistBranchCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<ChangePharmacistBranchCommandHandler> _logger;

        public ChangePharmacistBranchCommandHandler(
            IUnitOfWork unitOfWork,
            IResponseHandler responseHandler,
            ILogger<ChangePharmacistBranchCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(ChangePharmacistBranchCommand request,CancellationToken cancellationToken)
        {
            try
            {
                // 1. Get pharmacist
                var pharmacist = await _unitOfWork.Pharmacists.GetByUserIdAsync(request.PharmacistId ,true);

                if (pharmacist == null)
                    return _responseHandler.Failed<bool>("Pharmacist not found");

                // 2. Check new branch exists
                var branchExists = await _unitOfWork.Stores.GetByIdAsync(request.NewBranchId);

                if (branchExists is null)
                    return _responseHandler.Failed<bool>("Target branch not found");

                // 3. Prevent unnecessary update
                if (pharmacist.StoreId == request.NewBranchId)
                    return _responseHandler.Failed<bool>("Pharmacist already assigned to this branch");

                // 4. Update branch
                pharmacist.StoreId = request.NewBranchId;
                await _unitOfWork.SaveChangesAsync();

                return _responseHandler.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error changing pharmacist {PharmacistId} to branch {BranchId}",
                    request.PharmacistId, request.NewBranchId);

                return _responseHandler.Failed<bool>("Failed to change pharmacist branch");
            }
        }
    }
}
