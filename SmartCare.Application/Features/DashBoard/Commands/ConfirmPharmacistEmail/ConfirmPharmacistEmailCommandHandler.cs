using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Commands.ConfirmPharmacistEmail
{
    public class ConfirmPharmacistEmailCommandHandler : IRequestHandler<ConfirmPharmacistEmailCommand, Response<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IResponseHandler _responseHandler;
        private readonly ILogger<ConfirmPharmacistEmailCommandHandler> _logger;
        private readonly IEmailService _emailService;

        public ConfirmPharmacistEmailCommandHandler(IUnitOfWork unitOfWork, IResponseHandler responseHandler, ILogger<ConfirmPharmacistEmailCommandHandler> logger, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _responseHandler = responseHandler;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<Response<bool>> Handle(ConfirmPharmacistEmailCommand request, CancellationToken cancellationToken)
        {
            var pharmacist = await _unitOfWork.Pharmacists.GetByUserIdAsync(request.id ,true);
            if (pharmacist == null)
            {
                _logger.LogWarning("Pharmacist with user id {UserId} not found.", request.id);
                return _responseHandler.NotFound<bool>(SystemMessages.USER_NOT_FOUND);
            }
            var result = pharmacist.ConfirmEmail();
            await _unitOfWork.SaveChangesAsync();
            // Send Email notification to the pharmacist about the email confirmation
            var email = pharmacist.User.Email;
            var subject = "Email Confirmation";
            var body = "Your email has been successfully confirmed.Try Login Now !";
            await _emailService.SendEmailAsync( email , subject , body);
            return result ? _responseHandler.Success(true, SystemMessages.SUCCESS) : _responseHandler.BadRequest<bool>(SystemMessages.FAILED);
        }
    }
}
