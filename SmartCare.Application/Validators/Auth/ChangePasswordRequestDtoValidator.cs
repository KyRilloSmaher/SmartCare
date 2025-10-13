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
    public class ChangePasswordRequestDtoValidator : AbstractValidator<ChangePasswordRequestDto>
    {
        public ChangePasswordRequestDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .Must(p => Constants.IsValid(Constants.StringType.PASSWORD, p))
                .WithMessage("Password must contain upper/lower case letters, digits, symbols, and be at least 12 characters long.");
        }
    }
}
