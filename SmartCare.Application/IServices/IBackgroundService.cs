using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    public interface IBackgroundJobService
    {
        string Enqueue(Expression<Action> methodCall);
        string Schedule(Expression<Action> methodCall, TimeSpan delay);
    }
}
