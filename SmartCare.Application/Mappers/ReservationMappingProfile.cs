using AutoMapper;
using SmartCare.Application.DTOs.Reservation.Responses;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Mappers
{
    public class ReservationMappingProfile : Profile
    {
        public ReservationMappingProfile()
        {
            FromReservationToReservationResponse();
        }

        void FromReservationToReservationResponse()
        { 
            CreateMap<Reservation, ReservationResponseDto>()
                .ForMember(dest => dest.ReservationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.QuantityReserved, opt => opt.MapFrom(src => src.QuantityReserved))
                .ForMember(dest => dest.ReservedAt, opt => opt.MapFrom(src => src.ReservedAt))
                .ForMember(dest => dest.ExpiredAt, opt => opt.MapFrom(src => src.ExpiredAt));
        }
    }
}
