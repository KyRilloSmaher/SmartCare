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
using System.Threading;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Client.Handlers
{
    public class GetClientByIdHandler : IRequestHandler<GetClientByIdAsyncQuery, Response<ClientResponseDto?>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        private readonly string tag = CacheConstants.Client;

        public GetClientByIdHandler(
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

        public async Task<Response<ClientResponseDto?>> Handle(GetClientByIdAsyncQuery request, CancellationToken cancellationToken)
        {
            var id = request.id;
            if (string.IsNullOrWhiteSpace(id))
                return _responseHandler.BadRequest<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            string cacheKey = $"client_id_{id}";

            try
            {
                var cachedClient = await _redisCacheService.GetDataAsync<ClientResponseDto>(cacheKey, tag);
                if (cachedClient != null)
                    return _responseHandler.Success(cachedClient);
            }
            catch (Exception) { /* Redis logic shouldn't break the app */ }

            var client = await _unitOfWork.Clients.GetByIdAsync(id);
            if (client == null)
                return _responseHandler.NotFound<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            var clientDto = _mapper.Map<ClientResponseDto?>(client);

            await _redisCacheService.SetDataAsync(cacheKey, clientDto, tag, Time.Default);
            return _responseHandler.Success(clientDto);
        }
    }
}