using MultiTenantApp.Web.Interfaces;

namespace MultiTenantApp.Web.Services
{
    /// <summary>
    /// Decorator service that adds caching capabilities to any service method
    /// </summary>
    public class CacheDecorator
    {
        private readonly ICacheService? _cacheService;

        public CacheDecorator(ICacheService? cacheService = null)
        {
            _cacheService = cacheService;
        }

        /// <summary>
        /// Executes a function with caching. If the result is cached, returns it. Otherwise, executes the function and caches the result.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="factory">Function to execute if cache miss</param>
        /// <param name="expirationMinutes">Cache expiration in minutes (default: 60)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cached or freshly computed result</returns>
        public async Task<T> ExecuteWithCacheAsync<T>(
            string cacheKey,
            Func<Task<T>> factory,
            int expirationMinutes = 60,
            CancellationToken cancellationToken = default)
        {
            if (_cacheService == null)
            {
                return await factory();
            }

            return await _cacheService.GetOrSetAsync(
                cacheKey,
                factory,
                TimeSpan.FromMinutes(expirationMinutes),
                cancellationToken);
        }

        /// <summary>
        /// Invalidates cache by key
        /// </summary>
        public async Task InvalidateCacheAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            if (_cacheService != null)
            {
                await _cacheService.RemoveAsync(cacheKey, cancellationToken);
            }
        }

        /// <summary>
        /// Invalidates cache by pattern (e.g., "products:*")
        /// </summary>
        public async Task InvalidateCacheByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            if (_cacheService != null)
            {
                await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
            }
        }
    }
}
