using AutoMapper;
using SmartCare.Application.DTOs.Inventory.Request;
using SmartCare.Application.DTOs.Inventory.Response;
using SmartCare.Application.DTOs.Product.Responses;
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
            FromInventoryToProductResponseDtoForPharmacist();
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

        void FromInventoryToProductResponseDtoForPharmacist()  
        {
            CreateMap<Inventory, ProductResponseDtoForPharmacist>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product.ProductId))
                .ForMember(dest => dest.NameEn, opt => opt.MapFrom(src => src.Product.NameEn))
                .ForMember(dest => dest.NameAr, opt => opt.MapFrom(src => src.Product.NameAr))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Product.Description))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.DiscountPercentage, opt => opt.MapFrom(src => src.Product.DiscountPercentage))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Product.AverageRating))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.Product.IsAvailable))
                .ForMember(dest => dest.DosageForm, opt => opt.MapFrom(src => src.Product.DosageForm))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.StockQuantity))
                .ForMember(dest => dest.AvailableStock, opt => opt.MapFrom(src => src.AvailableStock))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.Product.Images.Select(img => img.Url).ToList()))
                .ForMember(dest => dest.PrimaryImageUrl, opt => opt.MapFrom(src => src.Product.Images.FirstOrDefault(img => img.IsPrimary).Url));
        }
        #endregion


    }
}
