using FluentValidation;
using SmartCare.Application.DTOs.Favorites.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Favourite
{
    public class CreateFavouriteRequestDtoValidation : AbstractValidator<CreateFavouriteRequestDto>
    {
        public CreateFavouriteRequestDtoValidation()
        {
            RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.")
            .NotEqual(Guid.Empty).WithMessage("ProductId must be a valid");

            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("ClientId is required.")
                .MaximumLength(100).WithMessage("ClientId must not exceed 100 characters.");


        }
    }
}
