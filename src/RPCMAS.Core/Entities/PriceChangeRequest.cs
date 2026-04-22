using RPCMAS.Core.Enums;

namespace RPCMAS.Core.Entities;

public class PriceChangeRequest
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = null!;
    public DateTime RequestDate { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public int RequestedById { get; set; }
    public User RequestedBy { get; set; } = null!;

    public ChangeType ChangeType { get; set; }
    public string? Reason { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Draft;

    public int? ApprovedById { get; set; }
    public User? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public int? RejectedById { get; set; }
    public User? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }

    public int? AppliedById { get; set; }
    public User? AppliedBy { get; set; }
    public DateTime? AppliedAt { get; set; }

    public int? CancelledById { get; set; }
    public User? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<PriceChangeRequestDetail> Details { get; set; } = new List<PriceChangeRequestDetail>();
}
