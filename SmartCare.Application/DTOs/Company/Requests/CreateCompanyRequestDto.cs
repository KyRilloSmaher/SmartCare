using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Companies.Requests
{
    public class CreateCompanyRequestDto
    {
        public string Name { get; set; }
        public IFormFile Logo { get; set; }
    }
}
