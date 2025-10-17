using FluentValidation;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.Validators.Address;
using SmartCare.Domain.Constants;
using SmartCare.Domain.IRepositories;
using System;

namespace SmartCare.Application.Validators.Auth
{
    public class SignUpRequestValidator : AbstractValidator<SignUpRequest>
    {
        private readonly IClientRepository _clientRepository;

        public SignUpRequestValidator(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .Must(u => Constants.IsValid(Constants.StringType.USERNAME, u))
                .WithMessage("Username must be 3–20 characters (letters, digits, underscores, or dots).")
                .MustAsync(async (u, cancellation) =>
                {
                    return await _clientRepository.IsClientnameUniqueAsync(u);
                })
                .WithMessage("Username is already taken!");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .Must(e => Constants.IsValid(Constants.StringType.EMAIL, e))
                .WithMessage("Invalid email format.")
                .MustAsync(async (email, cancellation) =>
                {
                    return await _clientRepository.IsEmailUniqueAsync(email);
                })
                .WithMessage("Email is already in use.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Must(p => Constants.IsValid(Constants.StringType.PHONE_NO, p))
                .WithMessage("Invalid phone number format.")
                .MustAsync(async (phone, cancellation) =>
                {
                    return await _clientRepository.IsClientPhoneNumberUniqueAsync(phone);
                })
                .WithMessage("Phone number is already in use.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .Must(p => Constants.IsValid(Constants.StringType.PASSWORD, p))
                .WithMessage("Password must contain upper/lowercase letters, digits, symbols, and be at least 12 characters long.");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required.")
                .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-13)))
                .WithMessage("You must be at least 13 years old to register.");

            RuleFor(x => x.ProfileImage)
                .NotNull().WithMessage("User image is required.")
                .Must(Constants.BeAValidImage)
                .WithMessage("Image must be jpg, jpeg, or png, and not exceed 5MB.");

            RuleFor(x => x.Address)
                .NotNull().WithMessage("Address is required.")
                .SetValidator(new CreateAddressRequestDtoValidator());
        }
    }
}
