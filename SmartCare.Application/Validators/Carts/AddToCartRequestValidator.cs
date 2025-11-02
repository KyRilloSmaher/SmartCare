using FluentValidation;
using SmartCare.Application.DTOs.Cart.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Carts
{
    public class AddToCartRequestValidator : AbstractValidator<AddToCartRequestDto>
    {
        public AddToCartRequestValidator()
        {
            RuleFor(x => x.CartId)
                .NotEmpty()
                .WithMessage("CartId is required.");

            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("ProductId is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.")
                .LessThanOrEqualTo(100)
                .WithMessage("Quantity cannot exceed 100 items per product.");
        }
    }
}
