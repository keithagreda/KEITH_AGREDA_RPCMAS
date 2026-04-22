using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPCMAS.Core.Entities;

namespace RPCMAS.Infrastructure.Data.Configurations;

public class PriceChangeRequestDetailConfiguration : IEntityTypeConfiguration<PriceChangeRequestDetail>
{
    public void Configure(EntityTypeBuilder<PriceChangeRequestDetail> b)
    {
        b.ToTable("PriceChangeRequestDetails");
        b.HasKey(x => x.Id);

        b.Property(x => x.SnapshotCurrentPrice).HasColumnType("decimal(18,2)");
        b.Property(x => x.ProposedNewPrice).HasColumnType("decimal(18,2)");
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.Ignore(x => x.MarkdownPercent);

        b.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.PriceChangeRequestId, x.ItemId }).IsUnique();
    }
}
