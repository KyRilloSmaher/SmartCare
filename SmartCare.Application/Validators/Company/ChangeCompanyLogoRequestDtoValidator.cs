using FluentValidation;
using Microsoft.AspNetCore.Http;
using SmartCare.Application.Companies.Requests;
using SmartCare.Application.DTOs.Caregory.Requests;
using SmartCare.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Comanies
{
    public class ChangeCompanyLogoRequestDtoValidator : AbstractValidator<ChangeCompanyLogoRequestDto>
    {
        public ChangeCompanyLogoRequestDtoValidator()
            {
            RuleFor(x => x.Image)
                .NotNull().WithMessage("Company image is required.")
                .Must(Constants.BeAValidImage).WithMessage("Image must be a valid image file (jpg, jpeg, png). With Maximum Size of 5MB");
            }


    }
}
