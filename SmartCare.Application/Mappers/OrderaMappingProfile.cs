using AutoMapper;
using SmartCare.Application.DTOs.Orders.Requests;
using SmartCare.Application.DTOs.Orders.Responses;
using SmartCare.Domain.Entities;

namespace SmartCare.Application.Mappers
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            FromOrderToOrderResponseDto();
            FromOrderItemToOrderItemResponse();
            FromCreateOrderRequestDtoToOrder();
            FromUpdateOrderRequestDtoToOrder();
        }

        private void FromOrderToOrderResponseDto()
        {
            CreateMap<Order, OrderResponseDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.StoreId, opt => opt.MapFrom(src => src.StoreId))
                .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.PaymentId))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClientId))
                .ReverseMap();
        }

        private void FromOrderItemToOrderItemResponse()
        {
            CreateMap<OrderItem, OrderItemResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
                .ForMember(dest => dest.InvetoryId, opt => opt.MapFrom(src => src.InvetoryId))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.SubTotal))
                .ForMember(dest => dest.product, opt => opt.MapFrom(src => src.Product))
                .ReverseMap();

            CreateMap<Product, ProductResponseForOrderDTo>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.NameEn, opt => opt.MapFrom(src => src.NameEn))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.DiscountPercentage, opt => opt.MapFrom(src => src.DiscountPercentage))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Images != null && src.Images.Any()
                    ? src.Images.First().Url
                    : string.Empty))
                .ReverseMap();
        }

        private void FromCreateOrderRequestDtoToOrder()
        {
            CreateMap<CreateOrderRequestDto, Order>()
                .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.clientId))
                .ForMember(dest => dest.StoreId, opt => opt.MapFrom(src => src.storeId))
                .ForMember(dest => dest.Address, opt => opt.Ignore()) // handled separately after creation
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore()) // calculated from items
                .ForMember(dest => dest.Payment, opt => opt.Ignore()) // created later
                .ForMember(dest => dest.Items, opt => opt.Ignore()) // populated from cart
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => Domain.Enums.OrderStatus.Pending));
        }

        private void FromUpdateOrderRequestDtoToOrder()
        {

            CreateMap<UpdateOrderRequestDto, Order>();
        }
    }
}
