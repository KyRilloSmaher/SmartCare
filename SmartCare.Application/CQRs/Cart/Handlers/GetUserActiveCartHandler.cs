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
    public class GetUserActiveCartHandler : IRequestHandler<GetUserActiveCartAsyncQuery, Response<CartResponseDto>>
    {
        #region Fields

        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetUserActiveCartHandler> _logger;

        #endregion
        public GetUserActiveCartHandler(IResponseHandler responseHandler, IMapper mapper, ILogger<GetUserActiveCartHandler> logger, IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        public async Task<Response<CartResponseDto>> Handle(GetUserActiveCartAsyncQuery request, CancellationToken cancellationToken)
        {
            var userId = request.userId;
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.BadRequest<CartResponseDto>(SystemMessages.BAD_REQUEST);

            var cart = await _unitOfWork.Carts.GetActiveCartAsync(userId);
            if (cart == null)
                return _responseHandler.NotFound<CartResponseDto>(SystemMessages.NOT_FOUND);

            var dto = _mapper.Map<CartResponseDto>(cart);
            return _responseHandler.Success(dto);
        }
    }
}
