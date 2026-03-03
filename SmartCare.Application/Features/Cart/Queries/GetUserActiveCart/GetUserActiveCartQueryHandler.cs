using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartCare.Application.DTOs.Cart.Responses;
using SmartCare.Application.Features.Carts.Queries.GetUserActiveCart;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.CQRs.Cart.Queries.GetUserActiveCart
{
    public class GetUserActiveCartQueryHandler : IRequestHandler<GetUserActiveCartQuery, Response<CartResponseDto>>
    {
        #region Fields

        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetUserActiveCartQueryHandler> _logger;

        #endregion
        public GetUserActiveCartQueryHandler(IResponseHandler responseHandler, IMapper mapper, ILogger<GetUserActiveCartQueryHandler> logger, IUnitOfWork unitOfWork)
        {
            _responseHandler = responseHandler;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        public async Task<Response<CartResponseDto>> Handle(GetUserActiveCartQuery request, CancellationToken cancellationToken)
        {
            var userId = request.userId;
            if (string.IsNullOrWhiteSpace(userId))
                return _responseHandler.BadRequest<CartResponseDto>(SystemMessages.BAD_REQUEST);

            var cart = await _unitOfWork.Carts.GetActiveCartAsync(userId,true);
            if (cart == null)
                return _responseHandler.NotFound<CartResponseDto>(SystemMessages.NOT_FOUND);
            cart.ReCalculateTotalPrice();
            await _unitOfWork.SaveChangesAsync();
            var dto = _mapper.Map<CartResponseDto>(cart);
            return _responseHandler.Success(dto);
        }
    }
}
