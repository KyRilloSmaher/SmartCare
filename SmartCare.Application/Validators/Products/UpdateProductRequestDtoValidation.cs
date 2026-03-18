using FluentValidation;
using Microsoft.AspNetCore.Http;
using SmartCare.Application.DTOs.Product.Requests;
using SmartCare.Domain.Constants;

namespace SmartCare.Application.Validators.Products
{
    public class UpdateProductRequestDtoValidation : AbstractValidator<UpdateProductRequestDto>
    {
        public UpdateProductRequestDtoValidation()
        {
            RuleFor(x => x.NameEn)
                .MaximumLength(100)
                .When(x => x.NameEn != null);
            RuleFor(x => x.NameAr)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description cannot be empty.")
                .When(x => x.Description != null);
            RuleFor(x => x.MedicalDescription)
                .NotEmpty()
                .When(x => x.MedicalDescription != null);
            RuleFor(x => x.Tags)
                .NotEmpty()
                .When(x => x.Tags != null);
            RuleFor(x => x.ActiveIngredients)
                .NotEmpty()
                .When(x => x.ActiveIngredients != null);

            RuleFor(x => x.SideEffects)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.SideEffects));

            RuleFor(x => x.Contraindications)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Contraindications));

            RuleFor(x => x.DosageForm)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.DosageForm));
            RuleFor(x => x.CategoryId)
                .NotEmpty()
                .When(x => x.CategoryId.HasValue);
            RuleFor(x => x.CompanyId)
                .NotEmpty()
                .When(x => x.CompanyId.HasValue);
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Price.HasValue);
            RuleFor(x => x.DiscountPercentage)
                .InclusiveBetween(0, 100)
                .When(x => x.DiscountPercentage.HasValue);
            RuleFor(x => x.NewMainImage)
                .Must(Constants.BeAValidImage)
                .When(x => x.NewMainImage != null)
                .WithMessage("Invalid main image format");
            RuleForEach(x => x.NewImages)
                .Must(Constants.BeAValidImage)
                .When(x => x.NewImages != null)
                .WithMessage("Invalid image format");

            RuleFor(x => x.RemoveImageIds)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .When(x => x.RemoveImageIds != null)
                .WithMessage("Duplicate image IDs are not allowed");
        }
    }
}