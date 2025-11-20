using Hangfire;
using SmartCare.Application.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.BackgroundJobImplemantations
{
    public class HangfireBackgroundJobService : IBackgroundJobService
    {
        public string Enqueue(Expression<Action> methodCall)
        {
            if (methodCall == null)
                throw new ArgumentNullException(nameof(methodCall));

            return BackgroundJob.Enqueue(methodCall);
        }

        public string Schedule(Expression<Action> methodCall, TimeSpan delay)
        {
            if (methodCall == null)
                throw new ArgumentNullException(nameof(methodCall));

            if (delay <= TimeSpan.Zero)
                throw new ArgumentException("Delay must be greater than zero.", nameof(delay));

            return BackgroundJob.Schedule(methodCall, delay);
        }

        // ----------------- Async non-generic -----------------
        public string Enqueue(Expression<Func<Task>> methodCall)
        {
            if (methodCall == null)
                throw new ArgumentNullException(nameof(methodCall));

            return BackgroundJob.Enqueue(methodCall);
        }

        public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay)
        {
            if (methodCall == null)
                throw new ArgumentNullException(nameof(methodCall));

            if (delay <= TimeSpan.Zero)
                throw new ArgumentException("Delay must be greater than zero.", nameof(delay));

            return BackgroundJob.Schedule(methodCall, delay);
        }

        // ----------------- Generic async -----------------
        public string Enqueue<TService>(Expression<Func<TService, Task>> methodCall) where TService : class
        {
            return BackgroundJob.Enqueue<TService>(methodCall);
        }

        public string Schedule<TService>(Expression<Func<TService, Task>> methodCall, TimeSpan delay) where TService : class
        {
            return BackgroundJob.Schedule<TService>(methodCall, delay);
        }
    }

}
