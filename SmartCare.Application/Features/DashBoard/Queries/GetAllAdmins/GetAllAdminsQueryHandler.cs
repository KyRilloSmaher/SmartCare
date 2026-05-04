using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Admins;
using SmartCare.Application.Features.DashBoard.Queries.GetAdminProfile;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Queries.GetAllAdmins
{
    public class GetAllAdminsQueryHandler : IRequestHandler<GetAllAdminsQuery, Response<List<AdminProfile>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllAdminsQueryHandler> _logger;
        private readonly IResponseHandler _responseHandler;
        public GetAllAdminsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAllAdminsQueryHandler> logger, IResponseHandler responseHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _responseHandler = responseHandler;
        }

        public async Task<Response<List<AdminProfile>>> Handle(GetAllAdminsQuery request, CancellationToken cancellationToken)
        {
            var admins = await _unitOfWork.UserManager.GetUsersInRoleAsync("DASHBOARD_ADMIN");
            if (admins == null || !admins.Any())
            {
                _logger.LogWarning("No admins found in the system.");
                return _responseHandler.NotFound<List<AdminProfile>>("No admins found.");
            }
            var adminProfiles = _mapper.Map<List<AdminProfile>>(admins);
            return _responseHandler.Success(adminProfiles, "Admins retrieved successfully.");
        }
    }
}
