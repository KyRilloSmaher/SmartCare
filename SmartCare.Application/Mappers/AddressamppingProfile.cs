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
            AddressToAddressResponseDto();
        }

        void CreateAddressRequestToAddress()
        {
            CreateMap<CreateAddressRequestDto, Address>();
        } 
        void AddressToAddressResponseDto()
        {
            CreateMap<Address, AddressResponseDto>();
        }
    }
}
