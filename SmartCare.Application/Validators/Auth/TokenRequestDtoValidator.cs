using FluentValidation;
using SmartCare.Application.DTOs.Auth.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Auth
{
    public class TokenRequestDtoValidator : AbstractValidator<TokenRequestDto>
    {
        public TokenRequestDtoValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("Access token is required.");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.");
        }
    }
}
