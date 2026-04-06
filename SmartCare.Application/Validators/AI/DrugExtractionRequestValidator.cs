using FluentValidation;
using SmartCare.Application.ExternalServiceInterfaces.AI.Request;
using SmartCare.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.AI
{
    public class DrugExtractionRequestValidator : AbstractValidator<DrugExtractionRequest>
    {

        public DrugExtractionRequestValidator()
        {
            RuleFor(x => x.Image)
                .Must(image => Constants.BeAValidImage(image))
                .WithMessage("image fileNot Valid.")
                .When(image => image != null);
        }

    }
}
