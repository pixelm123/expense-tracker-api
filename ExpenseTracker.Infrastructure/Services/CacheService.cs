using ExpenseTracker.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ExpenseTracker.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public CacheService(IMemoryCache cache) => _cache = cache;

    public T? Get<T>(string key)
        => _cache.TryGetValue(key, out T? value) ? value : default;

    public void Set<T>(string key, T value, TimeSpan expiry)
        => _cache.Set(key, value, expiry);

    public void Remove(string key)
        => _cache.Remove(key);
}
