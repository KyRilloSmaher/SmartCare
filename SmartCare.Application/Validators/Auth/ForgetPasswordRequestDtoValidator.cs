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
    public class ForgetPasswordRequestDtoValidator : AbstractValidator<ForgetPasswordRequestDto>
    {
        public ForgetPasswordRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .Must(e => Constants.IsValid(Constants.StringType.EMAIL, e))
                .WithMessage("Invalid email format.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .Must(p => Constants.IsValid(Constants.StringType.PASSWORD, p))
                .WithMessage("Invalid password format.");
        }
    }
}
