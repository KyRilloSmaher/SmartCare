using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Exceptions
{
    public class AiCoreValidationException : AiCoreException
    {
        public string Endpoint { get; }
        public AiCoreValidationException(string endpoint, string body)
            : base($"Validation error at {endpoint}: {body}")
            => Endpoint = endpoint;
    }
}
