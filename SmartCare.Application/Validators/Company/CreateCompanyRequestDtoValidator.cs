using FluentValidation;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Domain.Constants;

namespace SmartCare.Application.Validators.Companies
{
    public class CreateCompanyRequestDtoValidator : AbstractValidator<CreateCompanyRequestDto>
    {
        public CreateCompanyRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Company name is required.")
                .MaximumLength(100).WithMessage("Company name must not exceed 100 characters.");

            RuleFor(x => x.Logo)
                              .NotNull().WithMessage("Company image is required.")
                              .Must(Constants.BeAValidImage).WithMessage("Image must be a valid image file (jpg, jpeg, png). With Maximum Size of 5MB");
        }
    }
}
