using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Caregory.Requests
{
    public class ChangeCategoryLogoRequestDto
    {
        public Guid Id { get; set; }
        public IFormFile Image { get; set; } = null!;
    }
}
