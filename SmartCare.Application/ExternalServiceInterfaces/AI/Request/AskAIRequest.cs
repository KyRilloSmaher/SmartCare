using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.ExternalServiceInterfaces.AI.Request
{
    public class AskAIRequest
    {
        public IFormFile? AudioFile { get; set; } = null;
        public string? Question { get; set; } = null;
        public List<string>? ingredients { get; set; } = null;
    }
}
