using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.Features.DashBoard.Commands.AddAdmin
{
    public class AddAdminCommandHandler : IRequestHandler<AddAdminCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<AddAdminCommandHandler> _logger;

        public AddAdminCommandHandler(IUnitOfWork unitOfWork, IResponseHandler responseHandler, ILogger<AddAdminCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(AddAdminCommand request, CancellationToken cancellationToken)
        {
            // Validation checks
            var isEmailExists = await _unitOfWork.UserManager.FindByEmailAsync(request.Email);
            if (isEmailExists != null)
            {
                _logger.LogWarning("Signup failed - Email already exists: {Email}", request.Email);
                return _responseHandler.Failed<bool>(SystemMessages.EMAIL_ALREADY_EXISTS);
            }

            var Admin = new ApplictionUser {
             FirstName = request.FirstName,
             LastName = request.LastName,
             Email = request.Email,
             UserName = request.Email
            };
            var result = await _unitOfWork.UserManager.CreateAsync(Admin, request.Password);
            if (!result.Succeeded)
            {
                    _logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
            // Add role
            var roleResult = await _unitOfWork.UserManager.AddToRoleAsync(Admin, "DASHBOARD_ADMIN");
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Role assignment failed for {Email}", Admin.Email);
                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }

            await _unitOfWork.SaveChangesAsync();
            return _responseHandler.Success(true , SystemMessages.SUCCESS);
        }
    }
}
