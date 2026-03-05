using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Client.Queries;
using SmartCare.Application.DTOs.Client.Responses;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Client.Handlers
{
    public class GetAllClientsHandler : IRequestHandler<GetAllClientsAsyncQuery, Response<IEnumerable<ClientResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Client;

        public GetAllClientsHandler(
            IResponseHandler responseHandler,
            IBackgroundJobService backgroundJobService,
            IUnitOfWork unitOfWork,
            IRedisCacheService redisCacheService,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _backgroundJobService = backgroundJobService;
            _unitOfWork = unitOfWork;
            _redisCacheService = redisCacheService;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }
        #endregion

        public async Task<Response<IEnumerable<ClientResponseDto>>> Handle(GetAllClientsAsyncQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = "clients_all";

            try
            {
                var cachedClients = await _redisCacheService.GetDataAsync<IEnumerable<ClientResponseDto>>(cacheKey, tag);
                if (cachedClients != null)
                    return _responseHandler.Success(cachedClients);
            }
            catch (Exception) { }

            var clients = await _unitOfWork.Clients.GetAllAsync();
            var clientDtos = _mapper.Map<IEnumerable<ClientResponseDto>>(clients);

            await _redisCacheService.SetDataAsync(cacheKey, clientDtos, tag, Time.Default);
            return _responseHandler.Success(clientDtos);
        }
    }
}