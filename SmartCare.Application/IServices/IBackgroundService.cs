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
        string Enqueue(Expression<Func<Task>> methodCall);
        string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);
        // generic async version
        string Enqueue<TService>(Expression<Func<TService, Task>> methodCall) where TService : class;
        string Schedule<TService>(Expression<Func<TService, Task>> methodCall, TimeSpan delay) where TService : class;
    }
}
