using FluentValidation;
using SmartCare.Application.DTOs.Rates.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Rates
{
    public class UpdateRateRequestValidator : AbstractValidator<UpdateRateRequestDto>
    {
        public UpdateRateRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");

            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("ProductId is required.");

            RuleFor(x => x.Value)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating value must be between 1 and 5.");

            RuleFor(x => x.CreatedAt)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("CreatedAt cannot be in the future.");
        }
    }
}
