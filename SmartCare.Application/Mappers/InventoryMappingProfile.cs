using AutoMapper;
using SmartCare.Application.DTOs.Inventory.Request;
using SmartCare.Application.DTOs.Inventory.Response;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Mappers
{
    public class InventoryMappingProfile : Profile
    {
        public InventoryMappingProfile()
        {
            FromCreateInventoryRequestDtoToInventory();
            FromRemoveInventoryRequestDtoToInventory();
            FromUpdateInventoryRequestDtoToInventory();
            FromInventoryToInventoryUserResponseDto();
            FromInventoryToInventoryAdminResponseDto();
        }

        #region Request
        void FromCreateInventoryRequestDtoToInventory()
        {
            CreateMap<CreateInventoryRequestDto, Inventory>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.StoreId, opt => opt.MapFrom(src => src.StoreId))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.StockQuantity))
                .ForMember(dest => dest.ReservedQuantity, opt => opt.MapFrom(src => src.ReservedQuantity));
        }

        void FromRemoveInventoryRequestDtoToInventory()
        {
            CreateMap<RemoveInventoryRequestDto, Inventory>();

        }

        void FromUpdateInventoryRequestDtoToInventory()
        {
            CreateMap<UpdateInventoryRequestDto, Inventory>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.InventoryId));

        }
        #endregion


        #region Response
        void FromInventoryToInventoryAdminResponseDto()
        {
            
            CreateMap<Inventory, InventoryAdminResponseDto>()
                    .ForMember(dest => dest.InventoryId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                    .ForMember(dest => dest.StoreId, opt => opt.MapFrom(src => src.StoreId))
                    .ForMember(dest => dest.stockQuantity, opt => opt.MapFrom(src => src.StockQuantity))
                    .ForMember(dest => dest.ReservedQuantity, opt => opt.MapFrom(src => src.ReservedQuantity))
                    .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.StockQuantity - src.ReservedQuantity))
                    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.NameEn))
                    .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Store.Address))
                    .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Store.Phone));
        }

        void FromInventoryToInventoryUserResponseDto()
        {

            CreateMap<Inventory, InventoryUserResponseDto>()
                    .ForMember(dest => dest.InventoryId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                    .ForMember(dest => dest.StoreId, opt => opt.MapFrom(src => src.StoreId))
                    .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.StockQuantity - src.ReservedQuantity))
                    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.NameEn))
                    .ForMember(dest => dest.StoreName , opt => opt.MapFrom(src => src.Store.Name))
                    .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Store.Address))
                    .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Store.Phone));
        }
        #endregion


    }
}
