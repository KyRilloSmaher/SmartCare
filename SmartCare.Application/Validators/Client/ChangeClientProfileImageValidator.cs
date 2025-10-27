using FluentValidation;
using Microsoft.AspNetCore.Http;
using SmartCare.Application.DTOs.Client.Requests;
using SmartCare.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Validators.Client
{
    public class ChangeClientProfileImageValidator:AbstractValidator<ChangeClientProfileImageRequestDto>
    {
        public ChangeClientProfileImageValidator() {

            RuleFor(x => x.ProfileImage)
                    .NotNull().WithMessage("Profile image is required.")
                    .Must(Constants.BeAValidImage).WithMessage("Invalid image file. Allowed extensions: .jpg, .jpeg, .png, max size 5 MB.");
        }
       
    }
}
