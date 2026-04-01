using FluentValidation;
using SmartCare.Application.DTOs.Product.Requests;

public class VoiceSearchRequestValidator : AbstractValidator<VoiceSearchRequest>
{
    private readonly string[] _allowedExtensions = { ".mp3", ".wav", ".m4a" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public VoiceSearchRequestValidator()
    {
        RuleFor(x => x.AudioFile)
            .NotNull().WithMessage("Audio file is required.")

            .Must(file => file.Length > 0)
            .WithMessage("Audio file cannot be empty.")

            .Must(file => file.Length <= MaxFileSize)
            .WithMessage("File size must not exceed 10 MB.")

            .Must(file => HasValidExtension(file.FileName))
            .WithMessage("Only .mp3, .wav, .m4a files are allowed.")

            .Must(file => file.ContentType.StartsWith("audio/"))
            .WithMessage("Invalid file type. Must be audio.");
    }

    private bool HasValidExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        return _allowedExtensions.Contains(extension);
    }
}