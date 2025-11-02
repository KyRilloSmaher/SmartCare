using SmartCare.Application.DTOs.Reservation.Requests;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    public interface IReservationService
    {
        Task<Response<Reservation>> ReserveAsync(ReservationRequestDto reservation);


    }
}
