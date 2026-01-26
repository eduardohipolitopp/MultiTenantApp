using Hangfire;
using Microsoft.Extensions.Logging;

namespace MultiTenantApp.Infrastructure.Jobs
{
    /// <summary>
    /// Example recurring job using Hangfire.
    /// This demonstrates how to create background jobs.
    /// </summary>
    public class SampleRecurringJob
    {
        private readonly ILogger<SampleRecurringJob> _logger;

        public SampleRecurringJob(ILogger<SampleRecurringJob> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Example recurring job that runs every minute.
        /// </summary>
        [AutomaticRetry(Attempts = 3)]
        public void ProcessSomething()
        {
            _logger.LogInformation("Sample recurring job executed at {Time}", DateTime.UtcNow);
            // Add your job logic here
        }

        /// <summary>
        /// Example delayed job.
        /// </summary>
        [AutomaticRetry(Attempts = 3)]
        public void ProcessDelayedJob(string data)
        {
            _logger.LogInformation("Delayed job processed with data: {Data}", data);
            // Add your job logic here
        }
    }
}
