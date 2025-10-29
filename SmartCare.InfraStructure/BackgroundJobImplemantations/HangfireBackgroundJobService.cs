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

            string jobId = BackgroundJob.Enqueue(methodCall);
            return jobId;
        }

        public string Schedule(Expression<Action> methodCall, TimeSpan delay)
        {
            if (methodCall == null)
                throw new ArgumentNullException(nameof(methodCall));

            if (delay <= TimeSpan.Zero)
                throw new ArgumentException("Delay must be greater than zero.", nameof(delay));

            string jobId = BackgroundJob.Schedule(methodCall, delay);
            return jobId;
        }
    }
}
