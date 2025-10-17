using FluentValidation;
using Microsoft.AspNetCore.Http;
using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Category
{
    public class ChangeCompanyLogoRequestDtoValidator : AbstractValidator<ChangeCategoryLogoRequestDto>
    {
        public ChangeCompanyLogoRequestDtoValidator()
            {
            RuleFor(x => x.Image)
                .NotNull().WithMessage("Category image is required.")
                .Must(Constants.BeAValidImage).WithMessage("Image must be a valid image file (jpg, jpeg, png). With Maximum Size of 5MB");
            }


    }
}
