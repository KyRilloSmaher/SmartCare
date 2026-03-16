using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Exceptions
{
    public class CachedException : Exception
    {
        public CachedException(string message) : base(message)
        {
        }

        public CachedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
