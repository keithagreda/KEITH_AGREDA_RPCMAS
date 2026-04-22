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
                new User { Name = "Sam Supervisor (Men's Wear)", Role = UserRole.DepartmentSupervisor, DepartmentId = depts["Men's Wear"].Id },
                new User { Name = "Sara Supervisor (Ladies' Wear)", Role = UserRole.DepartmentSupervisor, DepartmentId = depts["Ladies' Wear"].Id },
                new User { Name = "Mark Manager (Merchandising)", Role = UserRole.MerchandisingManager },
                new User { Name = "Maria Manager (Merchandising)", Role = UserRole.MerchandisingManager },
                new User { Name = "Steve Storemgr", Role = UserRole.StoreManager }
            );
            await db.SaveChangesAsync(ct);
        }

        if (!await db.Items.AnyAsync(ct))
        {
            var items = BuildItems(depts);
            await db.Items.AddRangeAsync(items, ct);
            await db.SaveChangesAsync(ct);
        }
    }

    private static List<Item> BuildItems(Dictionary<string, Department> depts)
    {
        var items = new List<Item>(1100);

        AddVariants(items, depts["Men's Wear"], "Men's Polo Shirt", "Apparel", "MWP",
            brands: new[] { "PoloPro", "UrbanFit", "ClassicMen", "EastWear" },
            colors: new[] { "Black", "White", "Navy", "Red", "Olive", "Grey", "Maroon", "SkyBlue" },
            sizes: new[] { "XS", "S", "M", "L", "XL", "XXL" },
            basePrice: 599m, costRatio: 0.55m);

        AddVariants(items, depts["Ladies' Wear"], "Ladies Blouse", "Apparel", "LWB",
            brands: new[] { "Belle", "Floralia", "ChicLine", "AuroraSilk" },
            colors: new[] { "Pink", "Black", "White", "Lavender", "Mint", "Coral", "Peach", "Beige" },
            sizes: new[] { "XS", "S", "M", "L", "XL", "XXL" },
            basePrice: 749m, costRatio: 0.5m);

        AddVariants(items, depts["Shoes"], "Rubber Shoes", "Footwear", "SHR",
            brands: new[] { "RunMax", "AeroStride", "TrailGrip", "EveryDay" },
            colors: new[] { "Black", "White", "Grey", "RoyalBlue", "Green", "Red" },
            sizes: new[] { "5", "6", "7", "8", "9", "10", "11", "12" },
            basePrice: 1899m, costRatio: 0.6m);

        AddVariants(items, depts["Cosmetics"], "Lipstick", "Beauty", "CSL",
            brands: new[] { "GlowKiss", "VelvetMatte", "Lumiere", "RougeStudio" },
            colors: new[] { "Ruby", "Coral", "Berry", "Mauve", "Nude", "Pink", "Plum", "Rose", "Wine", "Brick", "Peach", "Cherry", "Sangria", "Toffee", "Spice", "Cinnamon", "Honey", "Caramel", "Chestnut", "Burgundy" },
            sizes: new[] { "Std" },
            basePrice: 349m, costRatio: 0.4m);

        AddVariants(items, depts["Housewares"], "Bedsheet Set", "Bedding", "HWB",
            brands: new[] { "ComfortHome", "DreamWeave", "PureCotton", "RoyalLinen" },
            colors: new[] { "Ivory", "Charcoal", "Sage", "Navy", "Blush", "Taupe" },
            sizes: new[] { "Twin", "Full", "Queen", "King", "California King" },
            basePrice: 1299m, costRatio: 0.55m);

        AddVariants(items, depts["Ladies' Wear"], "Handbag", "Accessories", "LWH",
            brands: new[] { "LuxBag", "UrbanCarry", "SatinTouch", "VogueLine" },
            colors: new[] { "Black", "Brown", "Beige", "Red", "Navy", "White", "Tan", "Blush", "Olive", "Burgundy" },
            sizes: new[] { "Mini", "Standard", "Tote" },
            basePrice: 2499m, costRatio: 0.5m);

        AddVariants(items, depts["Housewares"], "Non-stick Frying Pan", "Cookware", "HWP",
            brands: new[] { "ChefPro", "KitchenAce", "HeatGuard", "GranitX" },
            colors: new[] { "Black", "Red", "Grey", "Blue", "Copper" },
            sizes: new[] { "20cm", "22cm", "24cm", "26cm", "28cm", "30cm" },
            basePrice: 999m, costRatio: 0.5m);

        return items;
    }

    private static void AddVariants(
        List<Item> items, Department dept, string name, string category, string skuPrefix,
        string[] brands, string[] colors, string[] sizes,
        decimal basePrice, decimal costRatio)
    {
        var seq = 0;
        foreach (var brand in brands)
        foreach (var color in colors)
        foreach (var size in sizes)
        {
            seq++;
            var sku = $"{skuPrefix}-{Abbrev(brand)}-{Abbrev(color)}-{size}-{seq:D4}";
            items.Add(new Item
            {
                Sku = sku,
                Name = $"{brand} {name} {color} {size}",
                DepartmentId = dept.Id,
                Category = category,
                Brand = brand,
                Color = color,
                Size = size,
                CurrentPrice = basePrice,
                Cost = Math.Round(basePrice * costRatio, 2),
                Status = ItemStatus.Active
            });
        }
    }

    private static string Abbrev(string s)
    {
        var clean = new string(s.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        return clean.Length <= 4 ? clean : clean[..4];
    }
}
