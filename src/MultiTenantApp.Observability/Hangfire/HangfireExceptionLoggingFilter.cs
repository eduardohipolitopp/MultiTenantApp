using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace MultiTenantApp.Observability.Hangfire;

/// <summary>
/// Hangfire job filter that logs every job exception (including retries) to the application logger.
/// Register with GlobalJobFilters.Filters.Add(new HangfireExceptionLoggingFilter(logger)).
/// </summary>
public class HangfireExceptionLoggingFilter : IServerFilter
{
    private readonly ILogger<HangfireExceptionLoggingFilter> _logger;

    public HangfireExceptionLoggingFilter(ILogger<HangfireExceptionLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnPerforming(PerformingContext filterContext)
    {
        // Optional: could add scope with JobId for correlation
    }

    public void OnPerformed(PerformedContext filterContext)
    {
        if (filterContext.Exception == null)
            return;

        var jobId = filterContext.BackgroundJob.Id;
        var jobName = filterContext.BackgroundJob.Job?.Type?.Name ?? "Unknown";

        _logger.LogError(filterContext.Exception,
            "Hangfire job failed. JobId: {JobId}, Job: {JobName}",
            jobId,
            jobName);
    }
}
