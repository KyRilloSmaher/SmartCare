using AutoMapper;
using SmartCare.Application.DTOs.Auth.Requests;
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
    public class ApplicationUserMapper : Profile
    {
        public ApplicationUserMapper()
        {

            SignUpRequestToApplictionUser();
            PharmacistTOAplicationUser();
            RequestTOPharmacist();

            // Temporery 
            CreateMap<Pharmacist, PharmacistResponseDto>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Licence, opt => opt.MapFrom(src => src.LicenseNumber))
               .ForMember(dest => dest.PharmacistUserName, opt => opt.MapFrom(src => src.User.UserName))
               .ForMember(dest => dest.PharmacistEmail, opt => opt.MapFrom(src => src.User.Email))
               .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User.PhoneNumber))
               .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.StoreId))
               .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FirstName + src.User.LastName));
        }
        void SignUpRequestToApplictionUser()
        {
            CreateMap<SignUpRequest, ApplictionUser>()
           .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
           .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
           .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
           .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))

           .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
           .ForMember(dest => dest.Client, opt => opt.MapFrom(src => src));
        }
        void PharmacistTOAplicationUser() {
            CreateMap<AssignPharmacistRequest, ApplictionUser>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))

                //Ignore fields handled separately
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
        void  RequestTOPharmacist()
        {
            CreateMap<AssignPharmacistRequest, Pharmacist>()
                .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.LicenseNumber))
                //Will be assigned manually
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StoreId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Store, opt => opt.Ignore());
        }

    }
}
