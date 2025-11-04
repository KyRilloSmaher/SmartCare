using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Commons
{
    public interface ISqlLockManager
    {
        Task<IAsyncDisposable> AcquireLockAsync(string resource, string mode = "Exclusive", int timeoutMs = 10000);
    }

}
