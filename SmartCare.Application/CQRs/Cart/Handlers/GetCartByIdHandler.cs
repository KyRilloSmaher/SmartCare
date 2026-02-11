using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Cart.Queries;
using SmartCare.Application.DTOs.Cart.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Application.Messaging;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Cart.Handlers
{
    public class GetCartByIdHandler : IRequestHandler<GetCartByIdAsyncQuery , Response<CartResponseDto?>>
    {
        #region Fields

        private readonly IResponseHandler _responseHandler;
        private readonly ICartRepository _cartRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCartByIdHandler> _logger;


        #endregion
        public GetCartByIdHandler(IResponseHandler responseHandler, ICartRepository cartRepository, IMapper mapper, ILogger<GetCartByIdHandler> logger)
        {
            _responseHandler = responseHandler;
            _cartRepository = cartRepository;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<Response<CartResponseDto?>> Handle(GetCartByIdAsyncQuery request, CancellationToken cancellationToken)
        {
            var cartId = request.cartId;
            if (cartId == Guid.Empty)
                return _responseHandler.BadRequest<CartResponseDto?>(SystemMessages.BAD_REQUEST);

            var cart = await _cartRepository.GetByIdAsync(cartId);
            if (cart == null)
                return _responseHandler.NotFound<CartResponseDto?>(SystemMessages.NOT_FOUND);

            var dto = _mapper.Map<CartResponseDto?>(cart);
            return _responseHandler.Success(dto);
        }
    }
}
