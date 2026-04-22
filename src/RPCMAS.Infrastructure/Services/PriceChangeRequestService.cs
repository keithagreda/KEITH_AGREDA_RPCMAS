using Microsoft.EntityFrameworkCore;
using RPCMAS.Core.Common;
using RPCMAS.Core.Entities;
using RPCMAS.Core.Enums;
using RPCMAS.Core.Exceptions;
using RPCMAS.Core.Interfaces;
using RPCMAS.Infrastructure.Caching;
using RPCMAS.Infrastructure.Data;
using RPCMAS.Infrastructure.Services.Models;

namespace RPCMAS.Infrastructure.Services;

public class PriceChangeRequestService : IPriceChangeRequestService
{
    private readonly AppDbContext _db;
    private readonly IPriceChangeRequestRepository _repo;
    private readonly IItemRepository _itemRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;
    private readonly IRequestNumberGenerator _numberGen;

    public PriceChangeRequestService(
        AppDbContext db,
        IPriceChangeRequestRepository repo,
        IItemRepository itemRepo,
        IUnitOfWork uow,
        ICacheService cache,
        ICurrentUserService currentUser,
        IRequestNumberGenerator numberGen)
    {
        _db = db;
        _repo = repo;
        _itemRepo = itemRepo;
        _uow = uow;
        _cache = cache;
        _currentUser = currentUser;
        _numberGen = numberGen;
    }

    public Task<PagedResult<PriceChangeRequest>> ListAsync(PriceChangeRequestQuery query, CancellationToken ct = default)
        => _repo.QueryAsync(query, ct);

    public async Task<PriceChangeRequest> GetByIdAsync(int id, CancellationToken ct = default)
        => await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(PriceChangeRequest), id);

    public async Task<PriceChangeRequest> CreateAsync(CreateRequestInput input, CancellationToken ct = default)
    {
        if (input.Details is null || input.Details.Count == 0)
            throw new BusinessRuleException("REQUEST_NO_ITEMS", "A request must contain at least one item.");

        var items = await _itemRepo.GetByIdsAsync(input.Details.Select(d => d.ItemId), ct);
        var itemMap = items.ToDictionary(i => i.Id);

        var details = new List<PriceChangeRequestDetail>(input.Details.Count);
        foreach (var d in input.Details)
        {
            if (!itemMap.TryGetValue(d.ItemId, out var item))
                throw new NotFoundException(nameof(Item), d.ItemId);
            ValidateProposedPrice(item.CurrentPrice, d.ProposedNewPrice);

            details.Add(new PriceChangeRequestDetail
            {
                ItemId = item.Id,
                SnapshotCurrentPrice = item.CurrentPrice,
                ProposedNewPrice = d.ProposedNewPrice,
                EffectiveDate = d.EffectiveDate,
                Remarks = d.Remarks
            });
        }

        var now = DateTime.UtcNow;
        var request = new PriceChangeRequest
        {
            RequestNumber = await _numberGen.NextAsync(now, ct),
            RequestDate = now,
            DepartmentId = input.DepartmentId,
            RequestedById = _currentUser.UserId,
            ChangeType = input.ChangeType,
            Reason = input.Reason,
            Status = RequestStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now,
            Details = details
        };

        await _repo.AddAsync(request, ct);
        await _uow.SaveChangesAsync(ct);
        await InvalidateRequestListCacheAsync(ct);

        return await GetByIdAsync(request.Id, ct);
    }

    public async Task<PriceChangeRequest> UpdateAsync(int id, UpdateRequestInput input, CancellationToken ct = default)
    {
        var request = await GetByIdAsync(id, ct);
        EnsureStatus(request, RequestStatus.Draft, "Only Draft requests can be edited.");

        if (input.Details is null || input.Details.Count == 0)
            throw new BusinessRuleException("REQUEST_NO_ITEMS", "A request must contain at least one item.");

        request.ChangeType = input.ChangeType;
        request.Reason = input.Reason;
        request.UpdatedAt = DateTime.UtcNow;
        _db.Entry(request).Property(r => r.RowVersion).OriginalValue = input.RowVersion;

        var items = await _itemRepo.GetByIdsAsync(input.Details.Select(d => d.ItemId), ct);
        var itemMap = items.ToDictionary(i => i.Id);

        _db.PriceChangeRequestDetails.RemoveRange(request.Details);
        request.Details = new List<PriceChangeRequestDetail>();

        foreach (var d in input.Details)
        {
            if (!itemMap.TryGetValue(d.ItemId, out var item))
                throw new NotFoundException(nameof(Item), d.ItemId);
            ValidateProposedPrice(item.CurrentPrice, d.ProposedNewPrice);

            request.Details.Add(new PriceChangeRequestDetail
            {
                ItemId = item.Id,
                SnapshotCurrentPrice = item.CurrentPrice,
                ProposedNewPrice = d.ProposedNewPrice,
                EffectiveDate = d.EffectiveDate,
                Remarks = d.Remarks
            });
        }

        await _uow.SaveChangesAsync(ct);
        await InvalidateRequestListCacheAsync(ct);

        return await GetByIdAsync(request.Id, ct);
    }

    public Task SubmitAsync(int id, byte[] rowVersion, CancellationToken ct = default) =>
        TransitionAsync(id, rowVersion, RequestStatus.Draft, RequestStatus.Submitted,
            "Only Draft requests can be submitted.", r =>
            {
                if (r.Details.Count == 0)
                    throw new BusinessRuleException("REQUEST_NO_ITEMS", "A request must contain at least one item.");
            }, ct);

    public Task ApproveAsync(int id, byte[] rowVersion, CancellationToken ct = default) =>
        TransitionAsync(id, rowVersion, RequestStatus.Submitted, RequestStatus.Approved,
            "Only Submitted requests can be approved.", r =>
            {
                r.ApprovedById = _currentUser.UserId;
                r.ApprovedAt = DateTime.UtcNow;
            }, ct);

    public Task RejectAsync(int id, byte[] rowVersion, string reason, CancellationToken ct = default) =>
        TransitionAsync(id, rowVersion, RequestStatus.Submitted, RequestStatus.Rejected,
            "Only Submitted requests can be rejected.", r =>
            {
                r.RejectedById = _currentUser.UserId;
                r.RejectedAt = DateTime.UtcNow;
                r.RejectionReason = reason;
            }, ct);

    public Task CancelAsync(int id, byte[] rowVersion, CancellationToken ct = default) =>
        TransitionAsync(id, rowVersion, null, RequestStatus.Cancelled,
            "Cannot cancel a request in this status.", r =>
            {
                if (r.Status is RequestStatus.Applied or RequestStatus.Cancelled or RequestStatus.Rejected)
                    throw new BusinessRuleException("INVALID_STATUS", "Cannot cancel a request in this status.");
                r.CancelledById = _currentUser.UserId;
                r.CancelledAt = DateTime.UtcNow;
            }, ct);

    public async Task ApplyAsync(int id, byte[] rowVersion, CancellationToken ct = default)
    {
        var request = await GetByIdAsync(id, ct);
        EnsureStatus(request, RequestStatus.Approved, "Only Approved requests can be applied.");
        _db.Entry(request).Property(r => r.RowVersion).OriginalValue = rowVersion;

        await _uow.ExecuteInTransactionAsync(async token =>
        {
            foreach (var detail in request.Details)
            {
                var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == detail.ItemId, token)
                    ?? throw new NotFoundException(nameof(Item), detail.ItemId);
                item.CurrentPrice = detail.ProposedNewPrice;
            }

            request.Status = RequestStatus.Applied;
            request.AppliedById = _currentUser.UserId;
            request.AppliedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            await _uow.SaveChangesAsync(token);
        }, ct);

        await _cache.RemoveByPrefixAsync(CacheKeys.ItemListPrefix, ct);
        await _cache.RemoveByPrefixAsync(CacheKeys.ItemByIdPrefix, ct);
        await InvalidateRequestListCacheAsync(ct);
    }

    private async Task TransitionAsync(
        int id,
        byte[] rowVersion,
        RequestStatus? requiredFrom,
        RequestStatus to,
        string violationMessage,
        Action<PriceChangeRequest> mutate,
        CancellationToken ct)
    {
        var request = await GetByIdAsync(id, ct);
        if (requiredFrom.HasValue) EnsureStatus(request, requiredFrom.Value, violationMessage);
        _db.Entry(request).Property(r => r.RowVersion).OriginalValue = rowVersion;

        mutate(request);
        request.Status = to;
        request.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);
        await InvalidateRequestListCacheAsync(ct);
    }

    private static void EnsureStatus(PriceChangeRequest r, RequestStatus expected, string message)
    {
        if (r.Status != expected)
            throw new BusinessRuleException("INVALID_STATUS", message);
    }

    private static void ValidateProposedPrice(decimal currentPrice, decimal proposed)
    {
        if (proposed <= 0)
            throw new BusinessRuleException("INVALID_PRICE", "Proposed new price must be greater than zero.");
        if (proposed == currentPrice)
            throw new BusinessRuleException("INVALID_PRICE", "Proposed new price cannot equal the current price.");
    }

    private Task InvalidateRequestListCacheAsync(CancellationToken ct) =>
        _cache.RemoveByPrefixAsync(CacheKeys.RequestListPrefix, ct);
}
