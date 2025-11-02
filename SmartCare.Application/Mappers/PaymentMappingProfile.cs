using AutoMapper;
using SmartCare.Application.DTOs.Payment;
using SmartCare.Domain.Entities;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Mappers
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            // Map from Stripe Session to Payment entity
            CreateMap<Session, Payment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.AmountTotal / 100m))
                .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.Id));

            // Map from PaymentSessionRequest to Stripe session request (optional)
            CreateMap<PaymentSessionRequest, CreateCheckoutSessionRequest>();
        }
    }
}
