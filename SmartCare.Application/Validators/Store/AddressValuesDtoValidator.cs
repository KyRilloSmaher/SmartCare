using FluentValidation;
using SmartCare.Application.DTOs.Stores.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Store
{
    public class AddressValuesDtoValidator : AbstractValidator<AddressValuesDto>
    {
        public AddressValuesDtoValidator()
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .WithMessage("Latitude must be between -90 and 90 degrees.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage("Longitude must be between -180 and 180 degrees.");
        }
    }
}
