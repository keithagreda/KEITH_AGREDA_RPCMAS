using Microsoft.EntityFrameworkCore;
using RPCMAS.Core.Entities;

namespace RPCMAS.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<PriceChangeRequest> PriceChangeRequests => Set<PriceChangeRequest>();
    public DbSet<PriceChangeRequestDetail> PriceChangeRequestDetails => Set<PriceChangeRequestDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
