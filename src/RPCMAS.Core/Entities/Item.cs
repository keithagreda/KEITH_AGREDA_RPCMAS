using RPCMAS.Core.Enums;

namespace RPCMAS.Core.Entities;

public class Item
{
    public int Id { get; set; }
    public string Sku { get; set; } = null!;
    public string Name { get; set; } = null!;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public string Category { get; set; } = null!;
    public string Brand { get; set; } = null!;

    public string? Color { get; set; }
    public string? Size { get; set; }

    public decimal CurrentPrice { get; set; }
    public decimal Cost { get; set; }

    public ItemStatus Status { get; set; } = ItemStatus.Active;

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
