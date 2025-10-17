using AutoMapper;
using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Application.DTOs.Caregory.Response;
using SmartCare.Application.DTOs.Stores.Requests;
using SmartCare.Application.DTOs.Stores.Responses;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Mappers
{
    public class StoreMappingProfile :Profile
    {
        public StoreMappingProfile() { }


        void CreateStoreRequestDtoToStore()
        {
            CreateMap<CreateStoreRequestDto, Store>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

        }
        void UpdateStoreRequestDtoToStore()
        {
            CreateMap<UpdateStoreRequestDto, Store>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
        }
        void StoreToStoreResponseDto()
        {
            CreateMap<Store, StoreResponseDto>();
        }
        void StoreToStoreResponseForAdminDto()
        {
            CreateMap<Store, StoreResponseForAdminDto>();
        }
    }
}
