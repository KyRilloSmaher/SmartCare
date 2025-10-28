using AutoMapper;
using Microsoft.EntityFrameworkCore.Design;
using SmartCare.Application.DTOs.Favorites.Requests;
using SmartCare.Application.DTOs.Favorites.Responses;
using SmartCare.Application.DTOs.Rates.Responses;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Projection_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Mappers
{
    public class FavoriteMappingProfile : Profile
    {
        public FavoriteMappingProfile()
        {
            FavoriteToFavoriteResponseDto();
            CreateFavouriteRequestDtoToFavorite();
            ProductProjectionToFavoriteResponse();
        }

        void FavoriteToFavoriteResponseDto()
        {
            CreateMap<Favorite, FavoriteResponseDto>()
                                 .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product.ProductId))
                                 .ForMember(dest => dest.ProductNameEn, opt => opt.MapFrom(src => src.Product.NameEn))
                                 .ForMember(dest => dest.ProductNameAr, opt => opt.MapFrom(src => src.Product.NameAr))
                                 .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Product.Description))
                                 .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
                                 .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Product.AverageRating))
                                 .ForMember(dest => dest.TotalRatings, opt => opt.MapFrom(src => src.Product.TotalRatings))
                                 .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.Product.IsAvailable))
                                 .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src => src.Product.Images.FirstOrDefault(i => i.IsPrimary).Url));

        }

        void CreateFavouriteRequestDtoToFavorite()
        {
            CreateMap<CreateFavouriteRequestDto, Favorite>()
                .ForMember(sour => sour.ProductId, res => res.MapFrom(re => re.ProductId))
                .ForMember(sour => sour.ClientId, res => res.MapFrom(re => re.ClientId));


        }

        void ProductProjectionToFavoriteResponse()
            {
            CreateMap<ProductProjectionDTO, FavoriteResponseDto>();
            }
    }
}
