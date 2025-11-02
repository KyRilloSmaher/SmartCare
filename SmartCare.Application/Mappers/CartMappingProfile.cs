using AutoMapper;
using SmartCare.Domain.Entities;
using SmartCare.Application.DTOs.Cart.Requests;
using SmartCare.Application.DTOs.Cart.Responses;

namespace SmartCare.Application.Mappings
{
    public class CartMappingProfile : Profile
    {
        public CartMappingProfile()
        {

            FromCartToCartResponse();
            FromCartItemToCartItemResponse();
            FromUpdateCartItemRequestToCartItem();
            FromAddCartItemRequestToCartItem();

        }

        void FromCartToCartResponse()
        {
      
            CreateMap<Cart, CartResponseDto>()
                .ForMember(dest => dest.CartId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.ClientId))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        }

        void FromCartItemToCartItemResponse()
        {
            CreateMap<CartItem, CartItemResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CartItemId))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.NameEn : null))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.SubTotal))
                .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src => src.Product.Images.FirstOrDefault(i => i.IsPrimary).Url))
                .ForMember(dest => dest.ReservationId, opt => opt.MapFrom(src => src.ReservationId))
                .ForMember(dest => dest.ReservedUntil, opt => opt.MapFrom(src => src.Reservation != null ? src.Reservation.ExpiredAt : (DateTime?)null));
        }
        void FromUpdateCartItemRequestToCartItem()
        {
            CreateMap<UpdateCartItemRequestDto, CartItem>()
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.NewQuantity));

        }
        void FromAddCartItemRequestToCartItem()
        {
            CreateMap<AddToCartRequestDto, CartItem>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.CartId, opt => opt.MapFrom(src => src.CartId));

        }   
    }
}
