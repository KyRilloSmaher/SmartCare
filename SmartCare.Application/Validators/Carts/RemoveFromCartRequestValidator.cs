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
        }
    }
}
