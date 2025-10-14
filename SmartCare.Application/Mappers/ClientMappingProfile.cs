using AutoMapper;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.DTOs.Client.Requests;
using SmartCare.Application.DTOs.Client.Responses;
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
            ClientToClientDTO();
            UpdateClientRequestToClient();
        }

        void SignUpRequestToClient (){
            CreateMap<SignUpRequest, Client>()
                        .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore()) // handled after upload
                        .ForMember(dest => dest.Favorites, opt => opt.Ignore())
                        .ForMember(dest => dest.Orders, opt => opt.Ignore())
                        .ForMember(dest => dest.Rates, opt => opt.Ignore())
                        .ForMember(dest => dest.Cart, opt => opt.Ignore());
    
        }
        void ClientToClientDTO()
        {
            CreateMap<Client, ClientResponseDto>()
                .ForMember(dest =>dest.AccountType ,opt =>opt.MapFrom(src=>src.AccountType.ToString()))
                .ForMember(dest =>dest.Gender ,opt =>opt.MapFrom(src=>src.Gender.ToString()));

        }

        void UpdateClientRequestToClient()
        {
            CreateMap<UpdateClientRequest, Client>();
        }
    }
}
