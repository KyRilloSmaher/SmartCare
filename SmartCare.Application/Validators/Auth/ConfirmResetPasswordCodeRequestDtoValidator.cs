using FluentValidation;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Auth
{
    public class ConfirmResetPasswordCodeRequestDtoValidator : AbstractValidator<ConfirmResetPasswordCodeRequestDto>
    {
        public ConfirmResetPasswordCodeRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .Must(e => Constants.IsValid(Constants.StringType.EMAIL, e))
                .WithMessage("Invalid email format.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.")
                .Length(6).WithMessage("Code must be 6 digits long.");
        }
    }
}
