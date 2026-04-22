using RPCMAS.Core.Entities;
using RPCMAS.Core.Enums;

namespace RPCMAS.API.Dtos;

public record PriceChangeRequestSummaryDto(
    int Id,
    string RequestNumber,
    DateTime RequestDate,
    int DepartmentId,
    string DepartmentName,
    int RequestedById,
    string RequestedByName,
    ChangeType ChangeType,
    RequestStatus Status,
    int ItemCount,
    string RowVersion)
{
    public static PriceChangeRequestSummaryDto From(PriceChangeRequest r) => new(
        r.Id, r.RequestNumber, r.RequestDate,
        r.DepartmentId, r.Department?.Name ?? "",
        r.RequestedById, r.RequestedBy?.Name ?? "",
        r.ChangeType, r.Status,
        r.Details?.Count ?? 0,
        Convert.ToBase64String(r.RowVersion));
}

public record PriceChangeRequestDetailDto(
    int Id,
    int ItemId,
    string Sku,
    string ItemName,
    decimal CurrentPrice,
    decimal ProposedNewPrice,
    decimal MarkdownPercent,
    DateTime EffectiveDate,
    string? Remarks)
{
    public static PriceChangeRequestDetailDto From(PriceChangeRequestDetail d) => new(
        d.Id, d.ItemId, d.Item?.Sku ?? "", d.Item?.Name ?? "",
        d.SnapshotCurrentPrice, d.ProposedNewPrice, d.MarkdownPercent,
        d.EffectiveDate, d.Remarks);
}

public record PriceChangeRequestDto(
    int Id,
    string RequestNumber,
    DateTime RequestDate,
    int DepartmentId,
    string DepartmentName,
    int RequestedById,
    string RequestedByName,
    ChangeType ChangeType,
    string? Reason,
    RequestStatus Status,
    int? ApprovedById,
    string? ApprovedByName,
    DateTime? ApprovedAt,
    int? RejectedById,
    string? RejectedByName,
    DateTime? RejectedAt,
    string? RejectionReason,
    int? AppliedById,
    string? AppliedByName,
    DateTime? AppliedAt,
    int? CancelledById,
    string? CancelledByName,
    DateTime? CancelledAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string RowVersion,
    IReadOnlyList<PriceChangeRequestDetailDto> Details)
{
    public static PriceChangeRequestDto From(PriceChangeRequest r) => new(
        r.Id, r.RequestNumber, r.RequestDate,
        r.DepartmentId, r.Department?.Name ?? "",
        r.RequestedById, r.RequestedBy?.Name ?? "",
        r.ChangeType, r.Reason, r.Status,
        r.ApprovedById, r.ApprovedBy?.Name, r.ApprovedAt,
        r.RejectedById, r.RejectedBy?.Name, r.RejectedAt, r.RejectionReason,
        r.AppliedById, r.AppliedBy?.Name, r.AppliedAt,
        r.CancelledById, r.CancelledBy?.Name, r.CancelledAt,
        r.CreatedAt, r.UpdatedAt,
        Convert.ToBase64String(r.RowVersion),
        r.Details.Select(PriceChangeRequestDetailDto.From).ToList());
}

public class CreateRequestDto
{
    public int DepartmentId { get; set; }
    public ChangeType ChangeType { get; set; }
    public string? Reason { get; set; }
    public List<RequestDetailDto> Details { get; set; } = new();
}

public class UpdateRequestDto
{
    public ChangeType ChangeType { get; set; }
    public string? Reason { get; set; }
    public string RowVersion { get; set; } = "";
    public List<RequestDetailDto> Details { get; set; } = new();
}

public class RequestDetailDto
{
    public int ItemId { get; set; }
    public decimal ProposedNewPrice { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Remarks { get; set; }
}

public class WorkflowActionDto
{
    public string RowVersion { get; set; } = "";
}

public class RejectActionDto
{
    public string RowVersion { get; set; } = "";
    public string Reason { get; set; } = "";
}

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
