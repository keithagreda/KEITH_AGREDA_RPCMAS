using RPCMAS.Core.Common;
using RPCMAS.Core.Entities;
using RPCMAS.Core.Exceptions;
using RPCMAS.Core.Interfaces;
using RPCMAS.Infrastructure.Caching;

namespace RPCMAS.Infrastructure.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _repo;
    private readonly ICacheService _cache;

    public ItemService(IItemRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<PagedResult<Item>> ListAsync(ItemQuery query, CancellationToken ct = default)
    {
        var key = CacheKeys.ItemList(query.Search, query.Page, query.PageSize);
        var cached = await _cache.GetAsync<PagedResult<Item>>(key, ct);
        if (cached is not null) return cached;

        var result = await _repo.QueryAsync(query, ct);
        await _cache.SetAsync(key, result, TimeSpan.FromMinutes(5), ct);
        return result;
    }

    public async Task<Item> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var key = CacheKeys.ItemById(id);
        var cached = await _cache.GetAsync<Item>(key, ct);
        if (cached is not null) return cached;

        var item = await _repo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Item), id);

        await _cache.SetAsync(key, item, TimeSpan.FromMinutes(5), ct);
        return item;
    }
}
