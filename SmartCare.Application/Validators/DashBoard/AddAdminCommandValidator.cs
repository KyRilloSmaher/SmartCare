using FluentValidation;
using SmartCare.Application.Features.DashBoard.Commands.AddAdmin;
using SmartCare.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.DashBoard
{
    public class AddAdminCommandValidator : AbstractValidator<AddAdminCommand>
    {
        public AddAdminCommandValidator() {
            RuleFor(x => x.FirstName)
       .NotEmpty().WithMessage("First name is required.")
       .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .Must(e => Constants.IsValid(Constants.StringType.EMAIL, e))
                .WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .Must(p => Constants.IsValid(Constants.StringType.PASSWORD, p))
                .WithMessage("Password must contain upper/lowercase letters, digits, symbols, and be at least 12 characters long");
        }
    }
}
