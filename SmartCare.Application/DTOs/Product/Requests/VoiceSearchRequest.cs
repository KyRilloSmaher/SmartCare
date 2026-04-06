using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.DTOs.Product.Requests
{
    public class VoiceSearchRequest
    {
        public IFormFile AudioFile { get; set; }
    }
}
