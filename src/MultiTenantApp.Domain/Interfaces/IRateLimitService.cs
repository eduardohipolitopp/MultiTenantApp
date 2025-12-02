namespace MultiTenantApp.Domain.Interfaces
{
    public interface IRateLimitService
    {
        /// <summary>
        /// Checks if a request is allowed based on rate limit rules
        /// </summary>
        Task<RateLimitResult> CheckRateLimitAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets remaining requests for a key
        /// </summary>
        Task<int> GetRemainingRequestsAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default);
    }

    public class RateLimitResult
    {
        public bool IsAllowed { get; set; }
        public int Remaining { get; set; }
        public TimeSpan? RetryAfter { get; set; }
        public long RequestCount { get; set; }
    }
}
