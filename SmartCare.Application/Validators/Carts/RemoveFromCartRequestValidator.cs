using FluentValidation;
using SmartCare.Application.DTOs.Cart.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Carts
{
    public class RemoveFromCartRequestValidator : AbstractValidator<RemoveFromCartRequestDto>
    {
        public RemoveFromCartRequestValidator()
        {
            RuleFor(x => x.CartItemId)
                .NotEmpty()
                .WithMessage("CartItemId is required.");

            RuleFor(x => x.CartId)
                .NotEmpty()
                .WithMessage("CartId is required.");

            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("ProductId is required.");

            RuleFor(x => x.InventoryId)
                .NotEmpty()
                .WithMessage("InventoryId is required.");

            RuleFor(x => x.ReservationId)
                .NotEmpty()
                .WithMessage("ReservationId is required.");
        }
    }
}
