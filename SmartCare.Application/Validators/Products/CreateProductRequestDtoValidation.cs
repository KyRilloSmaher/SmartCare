using FluentValidation;
using SmartCare.Application.DTOs.Product.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Products
{
    public class CreateProductRequestDtoValidation : AbstractValidator<CreateProductRequestDto>
    {
        public CreateProductRequestDtoValidation()
        {

            RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("English name is required.")
            .MaximumLength(100);

            RuleFor(x => x.NameAr)
                .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");

            RuleFor(x => x.MedicalDescription)
                .NotEmpty().WithMessage("Medical description is required.");

            RuleFor(x => x.Tags)
                .NotEmpty().WithMessage("Tags are required.");

            RuleFor(x => x.ActiveIngredients)
                .NotEmpty().WithMessage("Active ingredients are required.");

            RuleFor(x => x.SideEffects)
                .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.SideEffects));

            RuleFor(x => x.Contraindications)
                .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Contraindications));

            RuleFor(x => x.DosageForm)
                .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.DosageForm));

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required.");

            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("CompanyId is required.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative.");

            RuleFor(x => x.DiscountPercentage)
                .InclusiveBetween(0, 100).WithMessage("Discount must be between 0 and 100.");

            RuleFor(x => x.ExpirationDate)
                .GreaterThan(DateTime.UtcNow).When(x => x.ExpirationDate.HasValue)
                .WithMessage("Expiration date must be in the future.");

            RuleFor(x => x.mainImage)
                .NotNull().WithMessage("Main image is required.");

            RuleFor(x => x.Images)
                .NotNull().WithMessage("Images list is required.")
                .Must(images => images.Count > 0).WithMessage("At least one additional image is required.");



        }
    }
}
