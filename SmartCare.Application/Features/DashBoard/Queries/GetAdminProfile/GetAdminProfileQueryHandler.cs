using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Admins;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Queries.GetAdminProfile
{
    public class GetAdminProfileQueryHandler : IRequestHandler<GetAdminProfileQuery, Response<AdminProfile>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAdminProfileQueryHandler> _logger;
        private readonly IResponseHandler _responseHandler;
        public GetAdminProfileQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAdminProfileQueryHandler> logger, IResponseHandler responseHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _responseHandler = responseHandler;
        }
        public async Task<Response<AdminProfile>> Handle(GetAdminProfileQuery request, CancellationToken cancellationToken)
        {
            var admin = await _unitOfWork.UserManager.FindByIdAsync(request.Id);
            if (admin is  null)
            {
                _logger.LogWarning("Admin with id {AdminId} not found.", request.Id);
                return _responseHandler.NotFound<AdminProfile>($"Admin with id {request.Id} not found.");
            }
            var adminProfile = _mapper.Map<AdminProfile>(admin);
            return _responseHandler.Success(adminProfile);
        }
    }
}
