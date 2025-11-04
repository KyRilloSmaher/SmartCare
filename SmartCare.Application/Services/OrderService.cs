using AutoMapper;
using SmartCare.Application.DTOs.Orders.Requests;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;

namespace SmartCare.Application.Services
{
    public class OrderService : IOrderService
    {
        #region Fields
        private readonly IResponseHandler _responseHandler;
        private readonly ICartRepository _cartRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IMapper _mapper;
        private readonly IBackgroundJobService _backgroundJobService;
        #endregion
        #region Constructor
        public OrderService(IResponseHandler responseHandler, ICartRepository cartRepository, IOrderRepository orderRepository, IReservationRepository reservationRepository, IProductRepository productRepository, IInventoryRepository inventoryRepository, IMapper mapper, IBackgroundJobService backgroundJobService, IClientRepository clientRepository)
        {
            _responseHandler = responseHandler;
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _reservationRepository = reservationRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
            _backgroundJobService = backgroundJobService;
            _clientRepository = clientRepository;
        }
        #endregion

        #region Methods
        public async  Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByCustomerIdAsync(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.BAD_REQUEST);

            }
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null)
            {
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.USER_NOT_FOUND);

            }
            var orders = await _orderRepository.GetOrdersByCustomerIdAsync(clientId);
            var ordersDto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(ordersDto);
        }

        public async  Task<Response<OrderResponseDto?>> GetOrderWithDetailsByIdAsync(Guid orderId)
        {
            if (orderId == Guid.Empty)
            {
                return _responseHandler.BadRequest<OrderResponseDto?>(SystemMessages.BAD_REQUEST);

            }
            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order == null)
            {
                return _responseHandler.NotFound<OrderResponseDto?>(SystemMessages.ORDER_NOT_FOUND);

            }
            var orderDto = _mapper.Map<OrderResponseDto?>(order);
            return _responseHandler.Success(orderDto);
        }

        public async  Task<Response<OrderResponseDto>> GetOrderByIdAsync(Guid orderId)
        {
            if (orderId == Guid.Empty)
            {
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.BAD_REQUEST);

            }
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                return _responseHandler.NotFound<OrderResponseDto>(SystemMessages.ORDER_NOT_FOUND);

            }
            var orderDto = _mapper.Map<OrderResponseDto>(order);

            return _responseHandler.Success(orderDto);
        }

        public async  Task<Response<int>> GetTotalOrdersCountAsync(Guid? storeId = null)
        {
            var count = await _orderRepository.GetTotalOrdersCountAsync(storeId);
            return _responseHandler.Success(count);
        }

        public async  Task<Response<decimal>> GetTotalRevenueAsync(Guid? storeId = null)
        {
            var revenue = await _orderRepository.GetTotalRevenueAsync(storeId);
            return _responseHandler.Success(revenue);
        }

        public async  Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByStatus(OrderStatus status, Guid? storeId = null)
        {
            if (!Enum.IsDefined(typeof(OrderStatus), status))
            {
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_ORDER_STATUS);

            }
            var orders = await _orderRepository.GetOrdersByStatus(status, storeId);
            var ordersDto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(ordersDto);
        }

        public async  Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersWithDetailsAsync()
        {
            var orders = await _orderRepository.GetOrdersWithDetailsAsync();
            var ordersDto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(ordersDto);
        }

        public async  Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? storeId = null)
        {
            if (startDate > endDate)
            {
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_DATE_RANGE);

            }
            var orders = await _orderRepository.GetOrdersByDateRangeAsync(startDate, endDate, storeId);
            var ordersDto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(ordersDto);

        }

        public async  Task<Response<IEnumerable<OrderResponseDto>>> GetOrdersByCustomerAndStatusAsync(string customerId, OrderStatus status)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.BAD_REQUEST);

            }
            if (!Enum.IsDefined(typeof(OrderStatus), status))
            {
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_ORDER_STATUS);

            }
            var orders = await _orderRepository.GetOrdersByCustomerAndStatusAsync(customerId, status);
            var ordersDto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(ordersDto);
        }

        public async  Task<Response<IEnumerable<OrderResponseDto>>> GetTopNOrdersByValueAsync(int n, Guid? storeId = null)
        {
            if (n <= 0)
            {
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_INPUT);

            }
            var orders = await _orderRepository.GetTopNOrdersByValueAsync(n, storeId);
            var ordersDto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(ordersDto);

        }

        public async  Task<Response<IEnumerable<OrderResponseDto>>> GetRecentOrdersAsync(int days, Guid? storeId = null)
        {
            if (days <= 0)
            {
                return _responseHandler.BadRequest<IEnumerable<OrderResponseDto>>(SystemMessages.INVALID_INPUT);

            }
            var orders = await _orderRepository.GetRecentOrdersAsync(days, storeId);
            var ordersDto = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
            return _responseHandler.Success(ordersDto);
        }

        public async  Task<Response<Dictionary<OrderStatus, int>>> GetOrderCountByStatusAsync(Guid? storeId = null)
        {
            var counts = await _orderRepository.GetOrderCountByStatusAsync(storeId);
            return _responseHandler.Success(counts);
        }

        public async  Task<Response<OrderResponseDto>> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            if (orderId == Guid.Empty)
            {
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.BAD_REQUEST);

            }
            if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
            {
                return _responseHandler.BadRequest<OrderResponseDto>(SystemMessages.INVALID_ORDER_STATUS);

            }
            var order = await _orderRepository.GetByIdAsync(orderId ,true);
            if (order == null)
            {
                return _responseHandler.NotFound<OrderResponseDto>(SystemMessages.ORDER_NOT_FOUND);

            }
            order.Status = newStatus;
            await _orderRepository.UpdateAsync(order);
            var orderDto = _mapper.Map<OrderResponseDto>(order);
            return _responseHandler.Success(orderDto);  
        }

        public async  Task<Response<OrderResponseDto>> CreateOrderAsync(CreateOrderRequestDto dto)
        {
            throw new NotImplementedException();
        }

        public async  Task<Response<OrderResponseDto>> UpdateOrderAsync(UpdateOrderRequestDto orderDto)
        {
            throw new NotImplementedException();
        }

        public async  Task<Response<bool>> DeleteOrderAsync(Guid orderId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
