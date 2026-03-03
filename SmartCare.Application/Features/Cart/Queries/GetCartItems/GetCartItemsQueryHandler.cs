using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using SmartCare.Application.commens;
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

namespace SmartCare.Application.CQRs.Cart.Queries.GetCartItems
{
    public class GetCartItemsQueryHandler : IRequestHandler<GetCartItemsQuery, Response<IEnumerable<CartItemResponseDto>>>
    {
        #region Fields

        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCartItemsQueryHandler> _logger;


        #endregion
        public GetCartItemsQueryHandler(IResponseHandler responseHandler, IMapper mapper, ILogger<GetCartItemsQueryHandler> logger, IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        public async Task<Response<IEnumerable<CartItemResponseDto>>> Handle(GetCartItemsQuery request, CancellationToken cancellationToken)
        {
            var cartId = request.cartId;
            _logger.LogDebug("GetCartItemsAsync called for CartId={CartId}", cartId);

            var cart = await _unitOfWork.Carts.EnsureCartExistsAsync(cartId);
            if (cart == null)
            {
                _logger.LogWarning("Cart not found for GetCartItemsAsync: {CartId}", cartId);
                return _responseHandler.NotFound<IEnumerable<CartItemResponseDto>>(SystemMessages.NOT_FOUND);
            }

            var items = await _unitOfWork.Carts.GetCartItemsAsync(cart.Id);
            var dto = _mapper.Map<IEnumerable<CartItemResponseDto>>(items);
            return _responseHandler.Success(dto);
        }
    }
}
