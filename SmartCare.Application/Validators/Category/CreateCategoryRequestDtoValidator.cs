using FluentValidation;
using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Domain.Constants;

namespace SmartCare.Application.Validators.Category
{
    public class CreateCompanyRequestDtoValidator : AbstractValidator<CreateCategoryRequestDto>
    {
        public CreateCompanyRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

            RuleFor(x => x.Logo)
                              .NotNull().WithMessage("Category image is required.")
                              .Must(Constants.BeAValidImage).WithMessage("Image must be a valid image file (jpg, jpeg, png). With Maximum Size of 5MB");
        }
    }
}
