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
        Task<string> EnqueueAsync(Expression<Action> methodCall);
        Task<string> ScheduleAsync(Expression<Action> methodCall, TimeSpan delay);
    }
}
