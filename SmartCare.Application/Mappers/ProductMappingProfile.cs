// SmartCare.Application/Mappers/ProductMappingProfile.cs
using AutoMapper;
using SmartCare.Application.DTOs.Contradictions.Response;
using SmartCare.Application.DTOs.Product.Requests;
using SmartCare.Application.DTOs.Product.Responses;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Projection_Models;

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
            ContradictionToContradictionDetailDto();
            ProductToContradictionDetail();
        }

        void FromProductToProductProjection()
        {
            CreateMap<Product, ProductProjectionDTO>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.ProductNameEn, opt => opt.MapFrom(src => src.NameEn))
                .ForMember(dest => dest.ProductNameAr, opt => opt.MapFrom(src => src.NameAr))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.AverageRating))
                .ForMember(dest => dest.TotalRatings, opt => opt.MapFrom(src => src.TotalRatings))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable))
                .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src =>
                    src.Images.FirstOrDefault(i => i.IsPrimary).Url));
        }

        void CreateProductRequestDtoToProduct()
        {
                CreateMap<CreateProductRequestDto, Product>()
                    .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                    .ForMember(dest => dest.Images, opt => opt.Ignore())
                    .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(_ => 0))
                    .ForMember(dest => dest.TotalRatings, opt => opt.MapFrom(_ => 0))
                    .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false))
                    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                    .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow)); 
        }

        void UpdateProductRequestDtoToProduct()
        {
            CreateMap<UpdateProductRequestDto, Product>()
                        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }

        void ProductToProductResponseDtoForClient()
        {
            CreateMap<Product, ProductResponseDtoForClient>()
                .ForMember(dest => dest.CompanyName,
                    opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : null))
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

        /// <summary>
        /// Maps Contradiction entity to ContradictionDetail DTO
        /// </summary>
        void ContradictionToContradictionDetailDto()
        {
            CreateMap<Contradiction, ContradictionDetail>()
                .ForMember(dest => dest.IngredientA, opt => opt.MapFrom(src => src.Ingredient_A))
                .ForMember(dest => dest.IngredientB, opt => opt.MapFrom(src => src.Ingredient_B))
                .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason))
                .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity))
                .ForMember(dest => dest.SeverityLevel, opt => opt.MapFrom(src =>
                    MapSeverityToLevel(src.Severity)));
        }

        /// <summary>
        /// Maps Product to ContradictionDetail for contradiction responses
        /// </summary>
        void ProductToContradictionDetail()
        {
            CreateMap<Product, ContradictionDetail>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.NameEn))
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src =>
                    src.Images.FirstOrDefault(i => i.IsPrimary).Url))
                .ForMember(dest => dest.IngredientA, opt => opt.Ignore()) // Set manually
                .ForMember(dest => dest.IngredientB, opt => opt.Ignore()) // Set manually
                .ForMember(dest => dest.Reason, opt => opt.Ignore())      // Set manually
                .ForMember(dest => dest.Severity, opt => opt.Ignore())    // Set manually
                .ForMember(dest => dest.SeverityLevel, opt => opt.Ignore()) // Set manually
                .ForMember(dest => dest.PurchaseDate, opt => opt.Ignore()); // Set manually
        }

        /// <summary>
        /// Helper method to map severity string to numeric level
        /// </summary>
        private int MapSeverityToLevel(string? severity)
        {
            return severity?.ToLower() switch
            {
                "high" => 3,
                "medium" => 2,
                "low" => 1,
                _ => 0
            };
        }
    }

}