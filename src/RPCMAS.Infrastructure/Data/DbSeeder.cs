using Microsoft.EntityFrameworkCore;
using RPCMAS.Core.Entities;
using RPCMAS.Core.Enums;

namespace RPCMAS.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        if (!await db.Departments.AnyAsync(ct))
        {
            db.Departments.AddRange(
                new Department { Name = "Men's Wear" },
                new Department { Name = "Ladies' Wear" },
                new Department { Name = "Shoes" },
                new Department { Name = "Cosmetics" },
                new Department { Name = "Housewares" }
            );
            await db.SaveChangesAsync(ct);
        }

        var depts = await db.Departments.ToDictionaryAsync(d => d.Name, ct);

        if (!await db.Users.AnyAsync(ct))
        {
            db.Users.AddRange(
                new User { Name = "Sam Supervisor (Men's Wear)",   Role = UserRole.DepartmentSupervisor, DepartmentId = depts["Men's Wear"].Id },
                new User { Name = "Sara Supervisor (Ladies' Wear)", Role = UserRole.DepartmentSupervisor, DepartmentId = depts["Ladies' Wear"].Id },
                new User { Name = "Mark Manager (Merchandising)",   Role = UserRole.MerchandisingManager },
                new User { Name = "Maria Manager (Merchandising)",  Role = UserRole.MerchandisingManager },
                new User { Name = "Steve Storemgr",                 Role = UserRole.StoreManager }
            );
            await db.SaveChangesAsync(ct);
        }

        if (!await db.Items.AnyAsync(ct))
        {
            var items = ItemSeeder.Build(depts);
            await db.Items.AddRangeAsync(items, ct);
            await db.SaveChangesAsync(ct);
        }
    }
}
