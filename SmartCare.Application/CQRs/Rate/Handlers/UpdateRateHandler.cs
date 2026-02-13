using AutoMapper;
using MediatR;
using SmartCare.Application.CQRs.Rate.Commands;
using SmartCare.Application.DTOs.Rates.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Rate.Handlers
{
    public class UpdateRateHandler : IRequestHandler<UpdateRateAsyncCommand, Response<RateResponseDto>>
    {
        #region Feilds
        private readonly IRateRepository _rateRepository;
        private readonly IProductRepository _productRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IMapper _mapper;
        private readonly IResponseHandler _responseHandler;
        string Rate_tag = CacheConstants.Rates;
        string Products_tag = CacheConstants.Products;

        #endregion

        public UpdateRateHandler(IRateRepository rateRepository, IProductRepository productRepository, IClientRepository clientRepository, IRedisCacheService redisCacheService, IMapper mapper, IResponseHandler responseHandler)
        {
            _rateRepository = rateRepository;
            _productRepository = productRepository;
            _clientRepository = clientRepository;
            _redisCacheService = redisCacheService;
            _mapper = mapper;
            _responseHandler = responseHandler;
        }

        public Task<Response<RateResponseDto>> Handle(UpdateRateAsyncCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
