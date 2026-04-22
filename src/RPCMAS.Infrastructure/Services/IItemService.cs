using RPCMAS.Core.Common;
using RPCMAS.Core.Entities;

namespace RPCMAS.Infrastructure.Services;

public interface IItemService
{
    Task<PagedResult<Item>> ListAsync(ItemQuery query, CancellationToken ct = default);
    Task<Item> GetByIdAsync(int id, CancellationToken ct = default);
}
