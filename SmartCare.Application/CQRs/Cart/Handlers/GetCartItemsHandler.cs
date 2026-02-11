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
using SmartCare.Application.CQRs.Cart.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Cart.Handlers
{
    public class GetCartItemsHandler : IRequestHandler<GetCartItemsAsyncQuery, Response<IEnumerable<CartItemResponseDto>>>
    {
        #region Fields

        private readonly IResponseHandler _responseHandler;
        private readonly ICartRepository _cartRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCartItemsHandler> _logger;


        #endregion
        public GetCartItemsHandler(IResponseHandler responseHandler, ICartRepository cartRepository, IMapper mapper, ILogger<GetCartItemsHandler> logger)
        {
            _responseHandler = responseHandler;
            _cartRepository = cartRepository;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<Response<IEnumerable<CartItemResponseDto>>> Handle(GetCartItemsAsyncQuery request, CancellationToken cancellationToken)
        {
            var cartId = request.cartId;
            _logger.LogDebug("GetCartItemsAsync called for CartId={CartId}", cartId);

            var cart = await _cartRepository.EnsureCartExistsAsync(cartId);
            if (cart == null)
            {
                _logger.LogWarning("Cart not found for GetCartItemsAsync: {CartId}", cartId);
                return _responseHandler.NotFound<IEnumerable<CartItemResponseDto>>(SystemMessages.NOT_FOUND);
            }

            var items = await _cartRepository.GetCartItemsAsync(cart.Id);
            var dto = _mapper.Map<IEnumerable<CartItemResponseDto>>(items);
            return _responseHandler.Success(dto);
        }
    }
}
