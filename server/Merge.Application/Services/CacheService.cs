using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using System.Collections.Concurrent;

namespace Merge.Application.Services;

// ✅ BOLUM 10.2: Redis distributed cache implementation
public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // ✅ PERFORMANCE FIX: Cache stampede protection - per-key semaphores
    // Note: For multi-instance deployments, use Redis-based distributed locks instead
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks = new();

    public CacheService(
        IDistributedCache distributedCache,
        ILogger<CacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    // ✅ PERFORMANCE FIX: ConfigureAwait(false) eklendi - thread pool starvation önleme
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(cachedValue))
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(cachedValue, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for key {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            var options = new DistributedCacheEntryOptions();

            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }
            else
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1); // Default 1 hour
            }

            await _distributedCache.SetStringAsync(key, json, options, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache write failed for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache remove failed for key {Key}", key);
        }
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        // ✅ PERFORMANCE FIX: Cache stampede protection - double-check locking pattern
        var cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
        if (cached != null)
        {
            return cached;
        }

        // ✅ PERFORMANCE FIX: Per-key semaphore to prevent cache stampede (thundering herd)
        // Only one request per key will execute the factory, others will wait and then check cache again
        var semaphore = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Double-check: Another request might have populated the cache while we were waiting
            cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
            if (cached != null)
            {
                return cached;
            }

            // Execute factory - only one request per key reaches here
            var value = await factory().ConfigureAwait(false);
            if (value != null)
            {
                await SetAsync(key, value, expiration, cancellationToken).ConfigureAwait(false);
            }

            return value;
        }
        finally
        {
            semaphore.Release();
            // Cleanup: Remove semaphore if no longer needed (optional optimization)
            if (semaphore.CurrentCount == 1 && _keyLocks.TryRemove(key, out var removedSemaphore))
            {
                removedSemaphore.Dispose();
            }
        }
    }

    public async Task<T?> GetOrCreateNullableAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        // ✅ PERFORMANCE FIX: Cache stampede protection - double-check locking pattern
        var cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
        if (cached != null)
        {
            return cached;
        }

        // ✅ PERFORMANCE FIX: Per-key semaphore to prevent cache stampede (thundering herd)
        var semaphore = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Double-check: Another request might have populated the cache while we were waiting
            cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
            if (cached != null)
            {
                return cached;
            }

            // Execute factory - only one request per key reaches here
            var value = await factory().ConfigureAwait(false);
            if (value != null)
            {
                await SetAsync(key, value, expiration, cancellationToken).ConfigureAwait(false);
            }

            return value;
        }
        finally
        {
            semaphore.Release();
            // Cleanup: Remove semaphore if no longer needed (optional optimization)
            if (semaphore.CurrentCount == 1 && _keyLocks.TryRemove(key, out var removedSemaphore))
            {
                removedSemaphore.Dispose();
            }
        }
    }
}

