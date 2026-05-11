using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCare.Application.commens;
using SmartCare.Application.CQRs.Order.Queries;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Application.Extentions;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.Features.Orders.Queries.GetOrdersWithDetails
{
    public class GetOrdersWithDetailsHandler : IRequestHandler<GetOrdersWithDetailsQuery, Response<PaginatedResult<OrderResponseDto>>>
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRedisCacheService _redisCacheService;
        private readonly string tag = CacheConstants.Orders;
        #endregion

        public GetOrdersWithDetailsHandler(
            IResponseHandler responseHandler,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IRedisCacheService redisCacheService)
        {
            _responseHandler = responseHandler;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _redisCacheService = redisCacheService;
        }

        public async Task<Response<PaginatedResult<OrderResponseDto>>> Handle(GetOrdersWithDetailsQuery request, CancellationToken cancellationToken)
        {
            var paramters = request.Request;

            if (paramters.PageNumber <= 0 || paramters.PageSize <= 0)
                return _responseHandler.BadRequest<PaginatedResult<OrderResponseDto>>(SystemMessages.INVALID_PAGINATION_PARAMETERS);
            var query = await _unitOfWork.Orders.GetOrdersWithDetailsAsync(paramters.ClientId , paramters.BranchId , paramters.PaymentMethod , paramters.OrderType , paramters.FromDate , paramters.ToDate);
            //var projectedQuery = _mapper.ProjectTo<OrderResponseDto>(query);
            //var paginatedResult = await projectedQuery.ToPaginatedListAsync(paramters.PageNumber,paramters.PageSize);



            // Step 1: paginate FIRST on entity
            var pagedOrders = await query
                .Skip((paramters.PageNumber - 1) * paramters.PageSize)
                .Take(paramters.PageSize)
                .ToListAsync();

            // Step 2: map in memory
            var mapped = _mapper.Map<List<OrderResponseDto>>(pagedOrders);

            // Step 3: count separately
            var count = await query.CountAsync();

            var paginatedResult = PaginatedResult<OrderResponseDto>.Success(
                mapped, count, paramters.PageNumber, paramters.PageSize
            );

            return _responseHandler.Success(paginatedResult);
        }
    }
}