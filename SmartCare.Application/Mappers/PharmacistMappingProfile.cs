using AutoMapper;
using SmartCare.Application.DTOs.Pharmacist.Request;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Mappers
{
    public class PharmacistMappingProfile : Profile
    {
        public PharmacistMappingProfile()
        {
            CreateMap<pharmacistSignUpRequestDto, ApplictionUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.userName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Pharmacist, opt => opt.MapFrom(src => src));

            CreateMap<pharmacistSignUpRequestDto, Pharmacist>()
                .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.LicenseNumber))
                .ForMember(dest => dest.StoreId, opt => opt.MapFrom(src => src.StoreId));


        }



    }
}
