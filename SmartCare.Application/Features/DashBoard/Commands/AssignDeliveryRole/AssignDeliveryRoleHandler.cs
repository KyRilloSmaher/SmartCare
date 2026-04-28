using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Commands.AssignDeliveryRole
{
    public class AssignDeliveryRoleHandler
       : IRequestHandler<AssignDeliveryRoleCommand, Response<bool>>
    {
        private readonly UserManager<ApplictionUser> _userManager;
        private readonly IResponseHandler _responseHandler;

        public AssignDeliveryRoleHandler(
            UserManager<ApplictionUser> userManager,
            IResponseHandler responseHandler)
        {
            _userManager = userManager;
            _responseHandler = responseHandler;
        }

        public async Task<Response<bool>> Handle(
            AssignDeliveryRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.ClientId);

            if (user is null)
                return _responseHandler.NotFound<bool>("User not found.");

            if (await _userManager.IsInRoleAsync(user, "DELIVERY"))
                return _responseHandler.BadRequest<bool>("User already has DELIVERY role.");

            var result = await _userManager.AddToRoleAsync(user, "DELIVERY");

            if (!result.Succeeded)
                return _responseHandler.BadRequest<bool>(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            return _responseHandler.Success(true, "DELIVERY role assigned successfully.");
        }
    }
}
