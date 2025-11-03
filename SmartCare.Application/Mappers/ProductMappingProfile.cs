using AutoMapper;
using SmartCare.Application.DTOs.Product.Requests;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Mappers
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            FromProductToProductProjection();
            CreateProductRequestDtoToProduct();
            UpdateProductRequestDtoToProduct();
            ProductToProductResponseDtoForClient();
            ProductToProductResponseDtoForManager();
        }

       void FromProductToProductProjection()
        {
                 CreateMap<Product,ProductProjectionDTO>()
                                 .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                                 .ForMember(dest => dest.ProductNameEn, opt => opt.MapFrom(src => src.NameEn))
                                 .ForMember(dest => dest.ProductNameAr, opt => opt.MapFrom(src => src.NameAr))
                                 .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                                 .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                                 .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.AverageRating))
                                 .ForMember(dest => dest.TotalRatings, opt => opt.MapFrom(src => src.TotalRatings))
                                 .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable))
                                 .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src => src.Images.FirstOrDefault(i => i.IsPrimary).Url));
        }

        void CreateProductRequestDtoToProduct()
        {
            CreateMap<CreateProductRequestDto, Product>();

        }

        void UpdateProductRequestDtoToProduct()
        {
            CreateMap<UpdateProductRequestDto, Product>();
        }

        void ProductToProductResponseDtoForClient()
        {
            CreateMap<Product, ProductResponseDtoForClient>()
     .ForMember(dest => dest.CompanyName,
         opt => opt.MapFrom(src => src.Company.Name))
     .ForMember(dest => dest.Images,
         opt => opt.MapFrom(src => src.Images
             .Where(i => !i.IsPrimary)
             .Select(i => i.Url)))
     .ForMember(dest => dest.MainImageUrl,
         opt => opt.MapFrom(src => src.Images
             .Where(p => p.IsPrimary)
             .Select(p => p.Url)
             .FirstOrDefault()));
        }
        void ProductToProductResponseDtoForManager()
        {
            CreateMap<Product, ProductResponseDtoForAdmin>();

        }
    }
}
