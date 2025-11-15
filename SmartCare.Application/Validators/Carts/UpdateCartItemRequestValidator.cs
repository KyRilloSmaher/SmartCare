using FluentValidation;
using SmartCare.Application.DTOs.Cart.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Carts
{
    public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequestDto>
    {
        public UpdateCartItemRequestValidator()
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


            RuleFor(x => x.NewQuantity)
                .GreaterThan(0)
                .WithMessage("NewQuantity must be greater than zero.")
                .LessThanOrEqualTo(100)
                .WithMessage("NewQuantity cannot exceed 100.");

        }
    }
}
