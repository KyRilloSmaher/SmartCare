using AutoMapper;
using SmartCare.Application.DTOs.Address.Requests;
using SmartCare.Application.DTOs.Address.Responses;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Mappers
{
    public class AddressamppingProfile : Profile
    {
        public AddressamppingProfile()
        {
            CreateAddressRequestToAddress();
            UpdateAddressRequestToAddress();
            AddressToAddressResponseDto();
        }

        void CreateAddressRequestToAddress()
        {
            CreateMap<CreateAddressRequestDto, Address>()
                .ForMember(dest=>dest.AddressLine , opt=>opt.MapFrom(src=>src.address));
        }
        void UpdateAddressRequestToAddress()
        {
            CreateMap<UpdateAddressRequestDto, Address>()
                 .ForMember(dest => dest.AddressLine, opt => opt.MapFrom(src => src.address));
        }
        void AddressToAddressResponseDto()
        {
            CreateMap<Address, AddressResponseDto>()
                .ForMember(dest => dest.address, opt => opt.MapFrom(src => src.AddressLine));
        }
    }
}
