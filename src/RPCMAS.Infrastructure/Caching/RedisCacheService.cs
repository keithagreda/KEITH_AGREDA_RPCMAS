using System.Text.Json;
using StackExchange.Redis;
using RPCMAS.Core.Interfaces;

namespace RPCMAS.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    };

    private readonly IConnectionMultiplexer _mux;

    public RedisCacheService(IConnectionMultiplexer mux) => _mux = mux;

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var db = _mux.GetDatabase();
        var value = await db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>((string)value!, JsonOpts);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var db = _mux.GetDatabase();
        var json = JsonSerializer.Serialize(value, JsonOpts);
        return db.StringSetAsync(key, json, ttl ?? DefaultTtl);
    }

    public Task RemoveAsync(string key, CancellationToken ct = default) =>
        _mux.GetDatabase().KeyDeleteAsync(key);

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var db = _mux.GetDatabase();
        foreach (var endpoint in _mux.GetEndPoints())
        {
            var server = _mux.GetServer(endpoint);
            if (!server.IsConnected || server.IsReplica) continue;

            var keys = server.Keys(database: db.Database, pattern: prefix + "*").ToArray();
            if (keys.Length == 0) continue;
            await db.KeyDeleteAsync(keys);
        }
    }
}

public static class CacheKeys
{
    public const string ItemListPrefix = "items:list:";
    public const string ItemByIdPrefix = "items:id:";
    public const string RequestListPrefix = "requests:list:";

    public static string ItemList(string? search, int page, int size) =>
        $"{ItemListPrefix}{search?.ToLowerInvariant() ?? ""}:{page}:{size}";

    public static string ItemById(int id) => $"{ItemByIdPrefix}{id}";

    public static string RequestList(string queryHash) => $"{RequestListPrefix}{queryHash}";
}
