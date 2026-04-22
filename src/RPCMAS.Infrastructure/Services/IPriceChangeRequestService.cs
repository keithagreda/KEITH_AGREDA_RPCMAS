using RPCMAS.Core.Common;
using RPCMAS.Core.Entities;
using RPCMAS.Infrastructure.Services.Models;

namespace RPCMAS.Infrastructure.Services;

public interface IPriceChangeRequestService
{
    Task<PagedResult<PriceChangeRequest>> ListAsync(PriceChangeRequestQuery query, CancellationToken ct = default);
    Task<PriceChangeRequest> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PriceChangeRequest> CreateAsync(CreateRequestInput input, CancellationToken ct = default);
    Task<PriceChangeRequest> UpdateAsync(int id, UpdateRequestInput input, CancellationToken ct = default);
    Task SubmitAsync(int id, byte[] rowVersion, CancellationToken ct = default);
    Task ApproveAsync(int id, byte[] rowVersion, CancellationToken ct = default);
    Task RejectAsync(int id, byte[] rowVersion, string reason, CancellationToken ct = default);
    Task ApplyAsync(int id, byte[] rowVersion, CancellationToken ct = default);
    Task CancelAsync(int id, byte[] rowVersion, CancellationToken ct = default);
}
