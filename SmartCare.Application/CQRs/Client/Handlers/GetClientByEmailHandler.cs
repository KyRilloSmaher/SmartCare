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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Client.Handlers
{
    public class GetClientByEmailHandler : IRequestHandler<GetClientByEmailAsyncQuery, Response<ClientResponseDto?>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IClientRepository _clientRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IRateRepository _rateRepository;
        private readonly IImageUploaderService _imageUploaderService;
        private readonly IMapper _mapper;
        string tag = CacheConstants.Client;

        #endregion

        public GetClientByEmailHandler(
            IResponseHandler responseHandler,
            IBackgroundJobService backgroundJobService,
            IClientRepository clientRepository,
            IRedisCacheService redisCacheService,
            IRateRepository rateRepository,
            IImageUploaderService imageUploaderService,
            IMapper mapper)
        {
            _responseHandler = responseHandler;
            _backgroundJobService = backgroundJobService;
            _clientRepository = clientRepository;
            _redisCacheService = redisCacheService;
            _rateRepository = rateRepository;
            _imageUploaderService = imageUploaderService;
            _mapper = mapper;
        }

        public async Task<Response<ClientResponseDto?>> Handle(GetClientByEmailAsyncQuery request, CancellationToken cancellationToken)
        {
            var email = request.email;
            if (string.IsNullOrWhiteSpace(email))
                return _responseHandler.BadRequest<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            string cacheKey = $"client_email_{email.ToLower()}";

            try
            {
                var cachedClient = await _redisCacheService.GetDataAsync<ClientResponseDto>(cacheKey, tag);
                if (cachedClient != null) return _responseHandler.Success(cachedClient);
            }
            catch (Exception) { }

            var client = await _clientRepository.GetByEmailAsync(email);
            if (client == null)
                return _responseHandler.NotFound<ClientResponseDto?>(SystemMessages.USER_NOT_FOUND);

            var clientDto = _mapper.Map<ClientResponseDto?>(client);

            await _redisCacheService.SetDataAsync(cacheKey, clientDto, tag, Time.Default);
            return _responseHandler.Success(clientDto);
        }
    }
}
