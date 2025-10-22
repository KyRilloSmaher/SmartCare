using AutoMapper;
using SmartCare.Application.DTOs.Rates.Requests;
using SmartCare.Application.DTOs.Rates.Responses;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Mappers
{
    public class RateMappingProfile : Profile
    {
        public RateMappingProfile()
        {
            RateToRateResponseDto();
            CreateRateRequestDtoToRate();
            UpdateRateRequestDtoToRate();
        }

        void RateToRateResponseDto()
        {
            CreateMap<Rate, RateResponseDto>();
        }

        void CreateRateRequestDtoToRate()
        {
            CreateMap<CreateRateRequestDto, Rate>();
        }
        void UpdateRateRequestDtoToRate() {
            CreateMap<UpdateRateRequestDto, Rate>();
        }
    }
}
