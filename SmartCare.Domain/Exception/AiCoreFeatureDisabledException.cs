using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Exceptions
{
    public class AiCoreFeatureDisabledException : AiCoreException
    {
        public string Endpoint { get; }
        public AiCoreFeatureDisabledException(string endpoint, string body)
            : base($"Feature disabled at {endpoint}: {body}")
            => Endpoint = endpoint;
    }
}
