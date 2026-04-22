using RPCMAS.Core.Enums;

namespace RPCMAS.Core.Common;

public class PriceChangeRequestQuery
{
    public string? RequestNumber { get; set; }
    public RequestStatus? Status { get; set; }
    public int? DepartmentId { get; set; }
    public ChangeType? ChangeType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
