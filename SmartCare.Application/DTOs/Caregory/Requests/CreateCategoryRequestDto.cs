using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Caregory.Requests
{
    public class CreateCategoryRequestDto
    {
        public string Name { get; set; }
        public IFormFile Logo { get; set; }
    }
}
