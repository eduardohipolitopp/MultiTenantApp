using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MultiTenantApp.Application.Configuration;
using MultiTenantApp.Domain.Interfaces;
using StackExchange.Redis;

namespace MultiTenantApp.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly CacheOptions _options;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(
            IConnectionMultiplexer redis,
            ILogger<RedisCacheService> logger,
            IOptions<CacheOptions> options)
        {
            _redis = redis;
            _database = redis.GetDatabase();
            _logger = logger;
            _options = options.Value;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var value = await _database.StringGetAsync(redisKey);

                if (!value.HasValue)
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return default;
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var serialized = JsonSerializer.Serialize(value, _jsonOptions);
                var expirationTime = expiration ?? TimeSpan.FromMinutes(_options.Defaults.DefaultExpirationMinutes);

                await _database.StringSetAsync(redisKey, serialized, expirationTime);
                _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}", key, expirationTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                await _database.KeyDeleteAsync(redisKey);
                _logger.LogDebug("Cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisPattern = GetRedisKey(pattern);
                var endpoints = _redis.GetEndPoints();
                
                foreach (var endpoint in endpoints)
                {
                    var server = _redis.GetServer(endpoint);
                    var keys = server.Keys(pattern: redisPattern);
                    
                    foreach (var key in keys)
                    {
                        await _database.KeyDeleteAsync(key);
                    }
                }
                
                _logger.LogDebug("Cache removed for pattern: {Pattern}", pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                return await _database.KeyExistsAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return false;
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var cached = await GetAsync<T>(key, cancellationToken);
            if (cached != null)
            {
                return cached;
            }

            var value = await factory();
            await SetAsync(key, value, expiration, cancellationToken);
            return value;
        }

        public async Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var result = await _database.StringIncrementAsync(redisKey, value);
                
                if (expiration.HasValue)
                {
                    await _database.KeyExpireAsync(redisKey, expiration.Value);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing cache for key: {Key}", key);
                return 0;
            }
        }

        private string GetRedisKey(string key)
        {
            return $"{_options.Redis.InstanceName}{key}";
        }
    }
}
