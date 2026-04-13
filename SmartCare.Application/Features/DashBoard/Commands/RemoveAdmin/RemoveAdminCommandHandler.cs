using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.Features.DashBoard.Commands.AddAdmin;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Commands.RemoveAdmin
{
    public class RemoveAdminCommandHandler : IRequestHandler<RemoveAdminCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<RemoveAdminCommandHandler> _logger;
        public RemoveAdminCommandHandler(IUnitOfWork unitOfWork, IResponseHandler responseHandler, ILogger<RemoveAdminCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(RemoveAdminCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.UserManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("User with email {Email} not found.", request.Email);
                return _responseHandler.Failed<bool>(SystemMessages.NOT_FOUND);
            }
            user.IsDeleted = true;
            var result = await _unitOfWork.UserManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Admin with email {Email} removed successfully.", request.Email);
                return _responseHandler.Success(true, SystemMessages.RECORD_DELETED);
            }
            else
            {
                _logger.LogError("Failed to remove admin with email {Email}. Errors: {Errors}", request.Email, result.Errors);
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}
