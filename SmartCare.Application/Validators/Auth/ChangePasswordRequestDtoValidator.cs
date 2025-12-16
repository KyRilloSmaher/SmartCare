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
                 .Custom((password, context) =>
                 {
                     var errors = Constants.GetPasswordErrors(password);

                     foreach (var error in errors)
                     {
                         context.AddFailure(error);
                     }
                 });

        }
    }
}
