using FluentValidation;
using SmartCare.Application.DTOs.Caregory.Requests;


namespace SmartCare.Application.Validators.Category
{
    public class UpdateCompanyRequestValidator : AbstractValidator<UpdateCategoryRequest>
    {
        public UpdateCompanyRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");
        }
    }
}
