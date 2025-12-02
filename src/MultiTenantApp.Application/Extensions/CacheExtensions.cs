using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Application.Extensions
{
    public static class CacheExtensions
    {
        /// <summary>
        /// Generates a cache key for a specific tenant
        /// </summary>
        public static string GetTenantCacheKey(this ICacheService cache, Guid tenantId, string key)
        {
            return $"tenant:{tenantId}:{key}";
        }

        /// <summary>
        /// Generates a cache key for a specific entity
        /// </summary>
        public static string GetEntityCacheKey<T>(this ICacheService cache, object id)
        {
            return $"{typeof(T).Name.ToLower()}:{id}";
        }

        /// <summary>
        /// Generates a cache key for a collection/list
        /// </summary>
        public static string GetCollectionCacheKey<T>(this ICacheService cache, string suffix = "")
        {
            var key = $"{typeof(T).Name.ToLower()}:list";
            return string.IsNullOrEmpty(suffix) ? key : $"{key}:{suffix}";
        }

        /// <summary>
        /// Invalidates cache for an entity and its collections
        /// </summary>
        public static async Task InvalidateEntityCacheAsync<T>(this ICacheService cache, object id, CancellationToken cancellationToken = default)
        {
            var entityKey = cache.GetEntityCacheKey<T>(id);
            await cache.RemoveAsync(entityKey, cancellationToken);

            var collectionPattern = cache.GetCollectionCacheKey<T>() + "*";
            await cache.RemoveByPatternAsync(collectionPattern, cancellationToken);
        }

        /// <summary>
        /// Gets a cached list with a factory function
        /// </summary>
        public static async Task<List<T>> GetOrSetListAsync<T>(
            this ICacheService cache, 
            string key, 
            Func<Task<List<T>>> factory, 
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default)
        {
            var cached = await cache.GetAsync<List<T>>(key, cancellationToken);
            if (cached != null && cached.Any())
            {
                return cached;
            }

            var items = await factory();
            if (items != null && items.Any())
            {
                await cache.SetAsync(key, items, expiration, cancellationToken);
            }

            return items ?? new List<T>();
        }
    }
}
