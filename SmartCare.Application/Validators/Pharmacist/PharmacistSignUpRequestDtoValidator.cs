using FluentValidation;
using SmartCare.Application.DTOs.Orders.Requests;
using SmartCare.Application.DTOs.Pharmacist.Request;
using SmartCare.Application.Validators.Address;
using SmartCare.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Pharmacist
{
    public class PharmacistSignUpRequestDtoValidator : AbstractValidator<pharmacistSignUpRequestDto>
    {
        public PharmacistSignUpRequestDtoValidator()
        {
            RuleFor(x => x.FirstName)
               .NotEmpty().WithMessage("First name is required.")
               .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.userName)
                .NotEmpty().WithMessage("Username is required.")
                .Must(u => Constants.IsValid(Constants.StringType.USERNAME, u))
                .WithMessage("Username must be 3–20 characters (letters, digits, underscores, or dots).")
              ;

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .Must(e => Constants.IsValid(Constants.StringType.EMAIL, e))
                .WithMessage("Invalid email format.")
                ;

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Must(p => Constants.IsValid(Constants.StringType.PHONE_NO, p))
                .WithMessage("Invalid phone number format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .Must(p => Constants.IsValid(Constants.StringType.PASSWORD, p))
                .WithMessage("Password must contain upper/lowercase letters, digits, symbols, and be at least 12 characters long.");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required.")
                .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-13)))
                .WithMessage("You must be at least 13 years old to register.");

            RuleFor(x => x.LicenseNumber)
                .NotEmpty().WithMessage("License number is required.")
                .Matches(@"^PH-\d{2}-[A-Z]{3}-\d{4}$").WithMessage("Invalid License Number format. Ex: PH-26-CAI-0842");

            RuleFor(x => x.StoreId)
                .NotEmpty().WithMessage("Store selection is required.");

        }
    }
}
