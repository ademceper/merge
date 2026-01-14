namespace Merge.Application.Interfaces;

// ✅ BOLUM 10.2: Distributed cache interface (Redis)
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    Task<T?> GetOrCreateNullableAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
}

