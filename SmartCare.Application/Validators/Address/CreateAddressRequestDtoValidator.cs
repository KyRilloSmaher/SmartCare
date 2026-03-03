using FluentValidation;
using SmartCare.Application.DTOs.Address.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Address
{
    public class CreateAddressRequestDtoValidator : AbstractValidator<CreateAddressRequestDto>
    {
        public CreateAddressRequestDtoValidator()
        {
            RuleFor(x => x.address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(200);

            RuleFor(x => x.Label)
                .NotEmpty().WithMessage("Label is required.")
                .MaximumLength(50);

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Invalid latitude value.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Invalid longitude value.");

            RuleFor(x => x.AdditionalInfo)
                .MaximumLength(250).When(x => !string.IsNullOrEmpty(x.AdditionalInfo));
        }
    }
}
