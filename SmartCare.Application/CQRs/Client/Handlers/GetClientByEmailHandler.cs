using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCare.Application.CQRs.Client.Queries;
using SmartCare.Application.DTOs.Client.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Client.Handlers
{
    public class GetClientByEmailHandler : IRequestHandler<GetClientByEmailAsyncQuery, Response<ClientResponseDto?>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private const string tag = CacheConstants.Client;
        #endregion

        public GetClientByEmailHandler(
            IResponseHandler responseHandler,
            IRedisCacheService redisCacheService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _redisCacheService = redisCacheService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<ClientResponseDto?>> Handle(GetClientByEmailAsyncQuery request, CancellationToken cancellationToken)
        {
            var email = request.email;
            if (string.IsNullOrWhiteSpace(email))
                return _responseHandler.BadRequest<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            string cacheKey = $"client_email_{email.ToLower()}";

            // Try to get from cache first
            try
            {
                var cachedClient = await _redisCacheService.GetDataAsync<ClientResponseDto>(cacheKey, tag);
                if (cachedClient != null)
                    return _responseHandler.Success(cachedClient);
            }
            catch { /* ignore cache failures */ }

            // Fetch user from Identity using UnitOfWork
            var user = await _unitOfWork.UserManager.FindByEmailAsync(email);
            if (user == null)
                return _responseHandler.NotFound<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            var clientDto = _mapper.Map<ClientResponseDto?>(user);

            // Save to cache
            try
            {
                await _redisCacheService.SetDataAsync(cacheKey, clientDto, tag, Time.Default);
            }
            catch { /* ignore cache failures */ }

            return _responseHandler.Success(clientDto);
        }
    }
}