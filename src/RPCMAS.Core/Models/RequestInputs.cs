using RPCMAS.Core.Enums;

namespace RPCMAS.Core.Models;

public class CreateRequestInput
{
    public int DepartmentId { get; set; }
    public ChangeType ChangeType { get; set; }
    public string? Reason { get; set; }
    public List<RequestDetailInput> Details { get; set; } = new();
}

public class RequestDetailInput
{
    public int ItemId { get; set; }
    public decimal ProposedNewPrice { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateRequestInput
{
    public ChangeType ChangeType { get; set; }
    public string? Reason { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public List<RequestDetailInput> Details { get; set; } = new();
}
