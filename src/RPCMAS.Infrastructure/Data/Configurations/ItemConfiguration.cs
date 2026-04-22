using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RPCMAS.Core.Entities;

namespace RPCMAS.Infrastructure.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> b)
    {
        b.ToTable("Items");
        b.HasKey(x => x.Id);

        b.Property(x => x.Sku).IsRequired().HasMaxLength(50);
        b.HasIndex(x => x.Sku).IsUnique();

        b.Property(x => x.Name).IsRequired().HasMaxLength(200);
        b.HasIndex(x => x.Name);

        b.Property(x => x.Category).IsRequired().HasMaxLength(100);
        b.Property(x => x.Brand).IsRequired().HasMaxLength(100);
        b.Property(x => x.Color).HasMaxLength(50);
        b.Property(x => x.Size).HasMaxLength(50);

        b.Property(x => x.CurrentPrice).HasColumnType("decimal(18,2)");
        b.Property(x => x.Cost).HasColumnType("decimal(18,2)");

        b.Property(x => x.Status).HasConversion<int>();
        b.HasIndex(x => x.Status);

        b.HasOne(x => x.Department)
            .WithMany(d => d.Items)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.RowVersion).IsRowVersion();
    }
}
