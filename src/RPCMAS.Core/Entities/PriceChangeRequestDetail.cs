namespace RPCMAS.Core.Entities;

public class PriceChangeRequestDetail
{
    public int Id { get; set; }

    public int PriceChangeRequestId { get; set; }
    public PriceChangeRequest PriceChangeRequest { get; set; } = null!;

    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public decimal SnapshotCurrentPrice { get; set; }
    public decimal ProposedNewPrice { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Remarks { get; set; }

    public decimal MarkdownPercent =>
        SnapshotCurrentPrice == 0
            ? 0m
            : Math.Round(((SnapshotCurrentPrice - ProposedNewPrice) / SnapshotCurrentPrice) * 100m, 2);
}
