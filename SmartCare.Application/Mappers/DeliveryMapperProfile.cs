using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Mappers
{
    public class DeliveryMapperProfile : Profile
    {
        public DeliveryMapperProfile()
        {
            CreateMap<Domain.Entities.ApplictionUser, DTOs.Delivery.DeliveryDto>().ReverseMap();
        }
    }
}
