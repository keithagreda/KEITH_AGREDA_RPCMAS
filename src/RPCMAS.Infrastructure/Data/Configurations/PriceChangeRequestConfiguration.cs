using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPCMAS.Core.Entities;

namespace RPCMAS.Infrastructure.Data.Configurations;

public class PriceChangeRequestConfiguration : IEntityTypeConfiguration<PriceChangeRequest>
{
    public void Configure(EntityTypeBuilder<PriceChangeRequest> b)
    {
        b.ToTable("PriceChangeRequests");
        b.HasKey(x => x.Id);

        b.Property(x => x.RequestNumber).IsRequired().HasMaxLength(30);
        b.HasIndex(x => x.RequestNumber).IsUnique();

        b.Property(x => x.ChangeType).HasConversion<int>();
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.Reason).HasMaxLength(1000);
        b.Property(x => x.RejectionReason).HasMaxLength(1000);

        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.RequestDate);
        b.HasIndex(x => new { x.DepartmentId, x.Status });

        b.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.RequestedBy)
            .WithMany()
            .HasForeignKey(x => x.RequestedById)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ApprovedBy)
            .WithMany()
            .HasForeignKey(x => x.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.RejectedBy)
            .WithMany()
            .HasForeignKey(x => x.RejectedById)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.AppliedBy)
            .WithMany()
            .HasForeignKey(x => x.AppliedById)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.CancelledBy)
            .WithMany()
            .HasForeignKey(x => x.CancelledById)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Details)
            .WithOne(d => d.PriceChangeRequest)
            .HasForeignKey(d => d.PriceChangeRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Property(x => x.RowVersion).IsRowVersion();
    }
}
