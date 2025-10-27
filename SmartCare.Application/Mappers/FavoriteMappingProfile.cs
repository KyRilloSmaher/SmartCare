using AutoMapper;
using Microsoft.EntityFrameworkCore.Design;
using SmartCare.Application.DTOs.Favorites.Requests;
using SmartCare.Application.DTOs.Favorites.Responses;
using SmartCare.Domain.Entities;
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
        }
        
        void FavoriteToFavoriteResponseDto()
        {
            CreateMap<Favorite, FavoriteResponseDto>()
                .ForMember(sour => sour.ProductId, res => res.MapFrom(re => re.Product.ProductId))
                .ForMember(sour => sour.ProductNameEn, res => res.MapFrom(re => re.Product.NameEn))
                .ForMember(sour => sour.ProductNameAr, res => res.MapFrom(re => re.Product.NameAr))
                .ForMember(sour => sour.Description_Of_Product, res => res.MapFrom(re => re.Product.Description))
                .ForMember(sour => sour.TotalRatings, res => res.MapFrom(re => re.Product.TotalRatings))
                .ForMember(sour => sour.Price, res => res.MapFrom(re => re.Product.Price))
                .ForMember(sour => sour.IsAvailable, res => res.MapFrom(re => re.Product.IsAvailable));

        }

        void CreateFavouriteRequestDtoToFavorite()
        {
            CreateMap<CreateFavouriteRequestDto, Favorite>()
                .ForMember(sour => sour.ProductId, res => res.MapFrom(re => re.ProductId))
                .ForMember(sour => sour.ClientId, res => res.MapFrom(re => re.ClientId));


        }
    }
}
