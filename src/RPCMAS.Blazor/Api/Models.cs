using RPCMAS.Core.Enums;

namespace RPCMAS.Blazor.Api;

public record ItemDto(
    int Id,
    string Sku,
    string Name,
    int DepartmentId,
    string DepartmentName,
    string Category,
    string Brand,
    string? Color,
    string? Size,
    decimal CurrentPrice,
    decimal Cost,
    ItemStatus Status,
    string RowVersion);

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
    string RowVersion);

public record PriceChangeRequestDetailDto(
    int Id,
    int ItemId,
    string Sku,
    string ItemName,
    decimal CurrentPrice,
    decimal ProposedNewPrice,
    decimal MarkdownPercent,
    DateTime EffectiveDate,
    string? Remarks);

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
    List<PriceChangeRequestDetailDto> Details);

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
    public string? Sku { get; set; }
    public string? ItemName { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal ProposedNewPrice { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.Today.AddDays(1);
    public string? Remarks { get; set; }

    public decimal MarkdownPercent =>
        CurrentPrice == 0 ? 0m : Math.Round(((CurrentPrice - ProposedNewPrice) / CurrentPrice) * 100m, 2);
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
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public record DepartmentLookup(int Id, string Name);
public record UserLookup(int Id, string Name, string Role, int? DepartmentId);
public record EnumLookup(int Id, string Name);

public class ApiException : Exception
{
    public int StatusCode { get; }
    public string? Code { get; }
    public IReadOnlyList<string> FieldErrors { get; }

    public ApiException(int statusCode, string message, string? code = null, IReadOnlyList<string>? fieldErrors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
        FieldErrors = fieldErrors ?? Array.Empty<string>();
    }
}
