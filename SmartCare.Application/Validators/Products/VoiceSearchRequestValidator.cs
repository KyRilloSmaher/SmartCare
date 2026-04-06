using FluentValidation;
using SmartCare.Application.DTOs.Product.Requests;
using SmartCare.Domain.Constants;

public class VoiceSearchRequestValidator : AbstractValidator<VoiceSearchRequest>
{

    public VoiceSearchRequestValidator()
    {
        RuleFor(x => x.AudioFile)
                 .Must(file => Constants.IsValidAudioFile(file))
                 .WithMessage("Audio file Not Valid.")
                 .When(File => File != null);
    }
}