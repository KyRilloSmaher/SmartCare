using AutoMapper;
using SmartCare.Application.DTOs.Pharmacist.Request;
using SmartCare.Application.DTOs.Pharmacist.Response;
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


            CreateMap<Pharmacist, PharmacistProfileDto>()
                .ForMember(dest => dest.FirstName,
                    opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName,
                    opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.Gender,
                    opt => opt.MapFrom(src => src.User.Gender))
                .ForMember(dest => dest.ProfileImageUrl,
                    opt => opt.MapFrom(src => src.User.ProfileImageUrl))
                .ForMember(dest => dest.StoreName,
                    opt => opt.MapFrom(src => src.Store != null ? src.Store.Name : string.Empty))
                .ForMember(dest => dest.StoreAddress,
                    opt => opt.MapFrom(src => src.Store != null ? src.Store.Address : string.Empty))
                .ForMember(dest => dest.StorePhone,
                    opt => opt.MapFrom(src => src.Store != null ? src.Store.Phone : string.Empty))
                .ForMember(dest => dest.LicenseNumber,
                    opt => opt.MapFrom(src => src.LicenseNumber))
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.StoreId,
                    opt => opt.MapFrom(src => src.StoreId));


        }



    }
}
