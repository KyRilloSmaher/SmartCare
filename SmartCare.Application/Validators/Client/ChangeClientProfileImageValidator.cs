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
                    .Must(BeAValidImage).WithMessage("Invalid image file. Allowed extensions: .jpg, .jpeg, .png, max size 5 MB.");
        }
        private bool BeAValidImage(IFormFile file)
        {
            if (file == null) return true;

            var ext = Path.GetExtension(file.FileName).ToLower();
            return Constants.AllowedImageExtensions.Contains(ext) && file.Length <= Constants.MaxImgSize;
        }
    }
}
