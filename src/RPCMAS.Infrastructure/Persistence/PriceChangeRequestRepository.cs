using Microsoft.EntityFrameworkCore;
using RPCMAS.Core.Common;
using RPCMAS.Core.Entities;
using RPCMAS.Core.Interfaces;
using RPCMAS.Infrastructure.Data;

namespace RPCMAS.Infrastructure.Persistence;

public class PriceChangeRequestRepository : IPriceChangeRequestRepository
{
    private readonly AppDbContext _db;

    public PriceChangeRequestRepository(AppDbContext db) => _db = db;

    public Task<PriceChangeRequest?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.PriceChangeRequests
            .Include(r => r.Department)
            .Include(r => r.RequestedBy)
            .Include(r => r.ApprovedBy)
            .Include(r => r.RejectedBy)
            .Include(r => r.AppliedBy)
            .Include(r => r.CancelledBy)
            .Include(r => r.Details).ThenInclude(d => d.Item)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<PagedResult<PriceChangeRequest>> QueryAsync(PriceChangeRequestQuery query, CancellationToken ct = default)
    {
        var q = _db.PriceChangeRequests
            .Include(r => r.Department)
            .Include(r => r.RequestedBy)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.RequestNumber))
            q = q.Where(r => r.RequestNumber.Contains(query.RequestNumber.Trim()));
        if (query.Status.HasValue) q = q.Where(r => r.Status == query.Status);
        if (query.DepartmentId.HasValue) q = q.Where(r => r.DepartmentId == query.DepartmentId);
        if (query.ChangeType.HasValue) q = q.Where(r => r.ChangeType == query.ChangeType);
        if (query.FromDate.HasValue) q = q.Where(r => r.RequestDate >= query.FromDate);
        if (query.ToDate.HasValue) q = q.Where(r => r.RequestDate <= query.ToDate);

        var total = await q.CountAsync(ct);
        var page = Math.Max(1, query.Page);
        var size = Math.Clamp(query.PageSize, 1, 200);

        var items = await q
            .OrderByDescending(r => r.RequestDate)
            .ThenByDescending(r => r.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new PagedResult<PriceChangeRequest>
        {
            Items = items,
            Page = page,
            PageSize = size,
            TotalCount = total
        };
    }

    public Task AddAsync(PriceChangeRequest request, CancellationToken ct = default) =>
        _db.PriceChangeRequests.AddAsync(request, ct).AsTask();

    public void Update(PriceChangeRequest request) => _db.PriceChangeRequests.Update(request);

    public Task<int> CountForDateAsync(DateTime date, CancellationToken ct = default)
    {
        var d = date.Date;
        var next = d.AddDays(1);
        return _db.PriceChangeRequests.CountAsync(r => r.RequestDate >= d && r.RequestDate < next, ct);
    }
}
