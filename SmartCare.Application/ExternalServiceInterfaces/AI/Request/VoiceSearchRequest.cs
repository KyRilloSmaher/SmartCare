using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.ExternalServiceInterfaces.AI.Request
{
    public record VoiceSearchRequest
    (
        IFormFile AudioFile,
        int TopK = 25
    );
}
