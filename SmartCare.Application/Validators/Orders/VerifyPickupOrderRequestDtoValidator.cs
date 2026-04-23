using FluentValidation;
using SmartCare.Application.DTOs.Orders.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Orders
{
    public class VerifyPickupOrderRequestDtoValidator : AbstractValidator<VerifyPickupOrderRequestDto>
    {
        public VerifyPickupOrderRequestDtoValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("Order ID is required")
                .NotEqual(Guid.Empty).WithMessage("Invalid Order Id format.");

            RuleFor(x => x.VerifyCode)
                .NotEmpty().WithMessage("Pickup code is required.")
                .Length(7).WithMessage("Pickup code must be exactly 7 digits.")
                .Matches(@"^[0-9]+$").WithMessage("Pickup code must contain Numbers only.");
        }
    }
}
