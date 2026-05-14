using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
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
        private readonly IUnitOfWork _unitOfWork;


        public AssignDeliveryRoleHandler(
            UserManager<ApplictionUser> userManager,
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<bool>> Handle(
            AssignDeliveryRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.ClientId);

            if (user is null)
                return _responseHandler.NotFound<bool>("User not found.");

            if (await _userManager.IsInRoleAsync(user, "DELIVERY"))
                return _responseHandler.BadRequest<bool>("User already has DELIVERY role.");

            // mashi 7alak
            var deliveryEntity = new Delivery { Id = user.Id };
            await _unitOfWork.Deliveries.AddAsync(deliveryEntity);

            var result = await _userManager.AddToRoleAsync(user, "DELIVERY");

            if (!result.Succeeded)
                return _responseHandler.BadRequest<bool>(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            return _responseHandler.Success(true, "DELIVERY role assigned successfully.");
        }
    }
}
