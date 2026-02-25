using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.CQRs.Authentication.Commands.Auth;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Authentication.Handlers.Auth
{
    public class LogoutHandler : IRequestHandler<LogoutAsyncCommand, Response<bool>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        #endregion

        public LogoutHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<bool>> Handle(LogoutAsyncCommand request, CancellationToken cancellationToken)
        {
            var userId = request.userId;

            // Fetch user via Identity from UnitOfWork
            var user = await _unitOfWork.UserManager.FindByIdAsync(userId);
            if (user == null)
                return _responseHandler.Failed<bool>(SystemMessages.USER_NOT_FOUND);

            try
            {
                // Clear refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;

                // Update security stamp to invalidate existing tokens
                await _unitOfWork.UserManager.UpdateSecurityStampAsync(user);

                // Update user
                var updateResult = await _unitOfWork.UserManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return _responseHandler.Failed<bool>(
                        string.Join(", ", updateResult.Errors.Select(e => e.Description))
                    );
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return _responseHandler.Success(true, SystemMessages.LOGOUT_SUCCESS);
            }
            catch (Exception ex)
            {

                return _responseHandler.Failed<bool>(SystemMessages.FAILED);
            }
        }
    }
}