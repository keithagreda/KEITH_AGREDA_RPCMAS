using RPCMAS.Core.Common;
using RPCMAS.Core.Entities;

namespace RPCMAS.Core.Interfaces;

public interface IPriceChangeRequestRepository
{
    Task<PriceChangeRequest?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<PriceChangeRequest>> QueryAsync(PriceChangeRequestQuery query, CancellationToken ct = default);
    Task AddAsync(PriceChangeRequest request, CancellationToken ct = default);
    void Update(PriceChangeRequest request);
    Task<int> CountForDateAsync(DateTime date, CancellationToken ct = default);
}
