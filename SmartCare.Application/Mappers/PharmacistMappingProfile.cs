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
            CreateMap<pharmacistSignUpRequestDto, Pharmacist>();
        }

        void CreatePharmacistSignupRequestDtoTopharmacist()
        {
            CreateMap<pharmacistSignUpRequestDto, Pharmacist>()
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.userName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));
        }
    }
}
