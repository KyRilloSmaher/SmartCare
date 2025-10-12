using AutoMapper;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Mappers
{
    public class ClientMappingProfile : Profile
    {
        public ClientMappingProfile() {
            SignUpRequestToClient();
        }

        void SignUpRequestToClient (){
            CreateMap<SignUpRequest, Client>()
                        .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore()) // handled after upload
                        .ForMember(dest => dest.birthDate, opt => opt.MapFrom(src => src.BirthDate))
                        .ForMember(dest => dest.Favorites, opt => opt.Ignore())
                        .ForMember(dest => dest.Orders, opt => opt.Ignore())
                        .ForMember(dest => dest.Rates, opt => opt.Ignore())
                        .ForMember(dest => dest.Cart, opt => opt.Ignore());
    
    }
}
}
