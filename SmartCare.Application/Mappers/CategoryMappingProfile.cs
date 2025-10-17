using AutoMapper;
using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Mappers
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            CreateCategoryRequestDtoToCategory();
            UpdateCategoryRequestDtoToCategory();
            CategoryToCategoryResponseDto();
            CategoryToCategoryResponseForAdminDto();

        }

        void CreateCategoryRequestDtoToCategory()
        {
            CreateMap<CreateCategoryRequestDto, Category>()
                .ForMember(dest => dest.LogoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

        }
        void UpdateCategoryRequestDtoToCategory()
        {
            CreateMap<UpdateCategoryRequest, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LogoUrl, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
        }
        void CategoryToCategoryResponseDto()
        {
            CreateMap<Category, CategoryResponseDto>();
        }
        void CategoryToCategoryResponseForAdminDto()
        {
            CreateMap<Category, CategoryResponseForAdminDto>();
        }
    }

}
