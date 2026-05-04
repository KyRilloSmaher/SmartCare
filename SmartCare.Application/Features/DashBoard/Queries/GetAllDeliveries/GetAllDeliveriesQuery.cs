using MediatR;
using SmartCare.Application.DTOs.Delivery;
using SmartCare.Application.Handlers.ResponseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Features.DashBoard.Queries.GetAllDeliveries
{
    public record GetAllDeliveriesQuery() : IRequest<Response<List<DeliveryDto>>>;
}
