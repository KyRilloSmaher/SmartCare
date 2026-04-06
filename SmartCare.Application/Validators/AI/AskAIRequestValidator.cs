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
    public class AskAIRequestValidator : AbstractValidator<AskAIRequest>
    {

        public AskAIRequestValidator()
        {
            RuleFor(x => x.AudioFile)
             .Must(file => file == null || Constants.IsValidAudioFile(file))
             .WithMessage("Audio file Not Valid.");
        }

    }
}