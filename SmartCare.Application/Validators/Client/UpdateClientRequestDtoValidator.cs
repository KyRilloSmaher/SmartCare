using FluentValidation;
using SmartCare.Application.DTOs.Client.Requests;
using SmartCare.Application.Validators.Address;
using SmartCare.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Client
{
    public class UpdateClientRequestDtoValidator: AbstractValidator<UpdateClientRequest>
    {
        public UpdateClientRequestDtoValidator() {

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50);

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .Must(u => Constants.IsValid(Constants.StringType.USERNAME, u))
                .WithMessage("Username must be 3-20 characters (letters, digits, underscores, or dots).");


            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Must(p => Constants.IsValid(Constants.StringType.PHONE_NO, p))
                .WithMessage("Invalid phone number format.");

            RuleFor(x => x.birthDate)
                .NotEmpty().WithMessage("Birth date is required.")
                .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-13)))
                .WithMessage("You must be at least 13 years old to register.");

        }
    }
}
