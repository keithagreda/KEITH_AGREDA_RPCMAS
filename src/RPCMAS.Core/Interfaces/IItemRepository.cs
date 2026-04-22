using RPCMAS.Core.Common;
using RPCMAS.Core.Entities;

namespace RPCMAS.Core.Interfaces;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Item?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<PagedResult<Item>> QueryAsync(ItemQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<Item>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
}
