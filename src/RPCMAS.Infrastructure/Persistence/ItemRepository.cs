using Microsoft.EntityFrameworkCore;
using RPCMAS.Core.Common;
using RPCMAS.Core.Entities;
using RPCMAS.Core.Interfaces;
using RPCMAS.Infrastructure.Data;

namespace RPCMAS.Infrastructure.Persistence;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _db;

    public ItemRepository(AppDbContext db) => _db = db;

    public Task<Item?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Items.Include(i => i.Department).FirstOrDefaultAsync(i => i.Id == id, ct);

    public Task<Item?> GetBySkuAsync(string sku, CancellationToken ct = default) =>
        _db.Items.Include(i => i.Department).FirstOrDefaultAsync(i => i.Sku == sku, ct);

    public async Task<IReadOnlyList<Item>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToArray();
        return await _db.Items.Where(i => idList.Contains(i.Id)).ToListAsync(ct);
    }

    public async Task<PagedResult<Item>> QueryAsync(ItemQuery query, CancellationToken ct = default)
    {
        var q = _db.Items.Include(i => i.Department).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(i => i.Sku.Contains(s) || i.Name.Contains(s));
        }

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var size = Math.Clamp(query.PageSize, 1, 200);

        var items = await q.OrderBy(i => i.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<Item>
        {
            Items = items,
            Page = page,
            PageSize = size,
            TotalCount = total
        };
    }
}
