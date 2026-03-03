using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Exceptions
{
    public class AiCoreException : Exception
    {
        public AiCoreException(string message) : base(message) { }
        public AiCoreException(string message, Exception inner) : base(message, inner) { }
    }
}
