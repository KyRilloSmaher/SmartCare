using FluentValidation;
using SmartCare.Application.DTOs.Reservation.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Reservation
{
    public class ReservationRequestValidator : AbstractValidator<ReservationRequestDto>
    {
        public ReservationRequestValidator()
        {
            RuleFor(r => r.CartItemId)
                .NotEmpty()
                .WithMessage("CartItemId is required.");

            RuleFor(r => r.QuantityReserved)
                .GreaterThan(0)
                .WithMessage("QuantityReserved must be greater than zero.");

            RuleFor(r => r.ReservedAt)
                .NotEmpty()
                .WithMessage("ReservedAt is required.")
                .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
                .WithMessage("ReservedAt cannot be set in the future.");
        }
    }
}
