using RPCMAS.Core.Common;
using RPCMAS.Core.Entities;

namespace RPCMAS.Core.Interfaces;

public interface IItemService
{
    Task<PagedResult<Item>> ListAsync(ItemQuery query, CancellationToken ct = default);
    Task<Item> GetByIdAsync(int id, CancellationToken ct = default);
}
