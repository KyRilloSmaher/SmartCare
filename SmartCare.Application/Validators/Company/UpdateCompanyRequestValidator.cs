using FluentValidation;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Caregory.Requests;


namespace SmartCare.Application.Validators.Companies
{
    public class UpdateCompanyRequestValidator : AbstractValidator<UpdateCompanyRequest>
    {
        public UpdateCompanyRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Company name is required.")
                .MaximumLength(100).WithMessage("Company name must not exceed 100 characters.");
        }
    }
}
