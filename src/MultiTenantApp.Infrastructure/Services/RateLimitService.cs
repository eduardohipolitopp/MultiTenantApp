using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MultiTenantApp.Application.Configuration;
using MultiTenantApp.Domain.Interfaces;
using StackExchange.Redis;

namespace MultiTenantApp.Infrastructure.Services
{
    public class RateLimitService : IRateLimitService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ILogger<RateLimitService> _logger;
        private readonly RateLimitOptions _options;

        public RateLimitService(
            IConnectionMultiplexer redis,
            ILogger<RateLimitService> logger,
            IOptions<RateLimitOptions> options)
        {
            _redis = redis;
            _database = redis.GetDatabase();
            _logger = logger;
            _options = options.Value;
        }

        public async Task<RateLimitResult> CheckRateLimitAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = $"ratelimit:{key}";
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var windowStart = now - (long)window.TotalSeconds;

                // Use sorted set with sliding window algorithm
                var transaction = _database.CreateTransaction();
                
                // Remove old entries
                var removeOldTask = transaction.SortedSetRemoveRangeByScoreAsync(redisKey, 0, windowStart);
                
                // Add current request
                var addTask = transaction.SortedSetAddAsync(redisKey, now, now);
                
                // Set expiration
                var expireTask = transaction.KeyExpireAsync(redisKey, window);
                
                // Get current count
                var countTask = transaction.SortedSetLengthAsync(redisKey);
                
                await transaction.ExecuteAsync();
                
                var requestCount = await countTask;
                
                var isAllowed = requestCount <= limit;
                var remaining = Math.Max(0, limit - (int)requestCount);
                
                TimeSpan? retryAfter = null;
                if (!isAllowed)
                {
                    // Get oldest entry in window to calculate retry after
                    var oldest = await _database.SortedSetRangeByScoreAsync(redisKey, windowStart, double.PositiveInfinity, take: 1);
                    if (oldest.Length > 0)
                    {
                        var oldestTime = long.Parse(oldest[0]!);
                        var secondsUntilReset = (oldestTime + (long)window.TotalSeconds) - now;
                        retryAfter = TimeSpan.FromSeconds(Math.Max(0, secondsUntilReset));
                    }
                }

                _logger.LogDebug(
                    "Rate limit check for {Key}: {RequestCount}/{Limit}, Allowed: {IsAllowed}", 
                    key, requestCount, limit, isAllowed);

                return new RateLimitResult
                {
                    IsAllowed = isAllowed,
                    Remaining = remaining,
                    RetryAfter = retryAfter,
                    RequestCount = requestCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rate limit for key: {Key}", key);
                // Fail open - allow the request if Redis is down
                return new RateLimitResult
                {
                    IsAllowed = true,
                    Remaining = limit,
                    RetryAfter = null,
                    RequestCount = 0
                };
            }
        }

        public async Task<int> GetRemainingRequestsAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = $"ratelimit:{key}";
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var windowStart = now - (long)window.TotalSeconds;

                var count = await _database.SortedSetLengthAsync(redisKey, windowStart, now);
                return Math.Max(0, limit - (int)count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting remaining requests for key: {Key}", key);
                return limit;
            }
        }
    }
}
