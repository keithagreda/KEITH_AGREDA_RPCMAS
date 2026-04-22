using RPCMAS.Core.Entities;
using RPCMAS.Core.Enums;

namespace RPCMAS.Infrastructure.Data;

/// <summary>
/// Generates &gt;1000 seed items per spec: 7 base products from the suggested list
/// expanded into variants by brand × color × size. Total = 1016 items.
/// </summary>
public static class ItemSeeder
{
    private record ProductTemplate(
        string DepartmentName,
        string Name,
        string Category,
        string SkuPrefix,
        string[] Brands,
        string[] Colors,
        string[] Sizes,
        decimal BasePrice,
        decimal CostRatio);

    private static readonly ProductTemplate[] Templates =
    {
        // Men's Polo Shirt — 4 × 8 × 6 = 192
        new("Men's Wear", "Men's Polo Shirt", "Apparel", "MWP",
            Brands: new[] { "PoloPro", "UrbanFit", "ClassicMen", "EastWear" },
            Colors: new[] { "Black", "White", "Navy", "Red", "Olive", "Grey", "Maroon", "SkyBlue" },
            Sizes:  new[] { "XS", "S", "M", "L", "XL", "XXL" },
            BasePrice: 599m, CostRatio: 0.55m),

        // Ladies Blouse — 4 × 8 × 6 = 192
        new("Ladies' Wear", "Ladies Blouse", "Apparel", "LWB",
            Brands: new[] { "Belle", "Floralia", "ChicLine", "AuroraSilk" },
            Colors: new[] { "Pink", "Black", "White", "Lavender", "Mint", "Coral", "Peach", "Beige" },
            Sizes:  new[] { "XS", "S", "M", "L", "XL", "XXL" },
            BasePrice: 749m, CostRatio: 0.50m),

        // Rubber Shoes — 4 × 6 × 8 = 192
        new("Shoes", "Rubber Shoes", "Footwear", "SHR",
            Brands: new[] { "RunMax", "AeroStride", "TrailGrip", "EveryDay" },
            Colors: new[] { "Black", "White", "Grey", "RoyalBlue", "Green", "Red" },
            Sizes:  new[] { "5", "6", "7", "8", "9", "10", "11", "12" },
            BasePrice: 1899m, CostRatio: 0.60m),

        // Lipstick — 4 × 20 × 1 = 80
        new("Cosmetics", "Lipstick", "Beauty", "CSL",
            Brands: new[] { "GlowKiss", "VelvetMatte", "Lumiere", "RougeStudio" },
            Colors: new[] {
                "Ruby", "Coral", "Berry", "Mauve", "Nude", "Pink", "Plum", "Rose",
                "Wine", "Brick", "Peach", "Cherry", "Sangria", "Toffee", "Spice",
                "Cinnamon", "Honey", "Caramel", "Chestnut", "Burgundy"
            },
            Sizes:  new[] { "Std" },
            BasePrice: 349m, CostRatio: 0.40m),

        // Bedsheet Set — 4 × 6 × 5 = 120
        new("Housewares", "Bedsheet Set", "Bedding", "HWB",
            Brands: new[] { "ComfortHome", "DreamWeave", "PureCotton", "RoyalLinen" },
            Colors: new[] { "Ivory", "Charcoal", "Sage", "Navy", "Blush", "Taupe" },
            Sizes:  new[] { "Twin", "Full", "Queen", "King", "California King" },
            BasePrice: 1299m, CostRatio: 0.55m),

        // Handbag — 4 × 10 × 3 = 120
        new("Ladies' Wear", "Handbag", "Accessories", "LWH",
            Brands: new[] { "LuxBag", "UrbanCarry", "SatinTouch", "VogueLine" },
            Colors: new[] { "Black", "Brown", "Beige", "Red", "Navy", "White", "Tan", "Blush", "Olive", "Burgundy" },
            Sizes:  new[] { "Mini", "Standard", "Tote" },
            BasePrice: 2499m, CostRatio: 0.50m),

        // Non-stick Frying Pan — 4 × 5 × 6 = 120
        new("Housewares", "Non-stick Frying Pan", "Cookware", "HWP",
            Brands: new[] { "ChefPro", "KitchenAce", "HeatGuard", "GranitX" },
            Colors: new[] { "Black", "Red", "Grey", "Blue", "Copper" },
            Sizes:  new[] { "20cm", "22cm", "24cm", "26cm", "28cm", "30cm" },
            BasePrice: 999m, CostRatio: 0.50m),
    };

    public static List<Item> Build(IReadOnlyDictionary<string, Department> departments)
    {
        var items = new List<Item>(capacity: 1100);

        foreach (var t in Templates)
        {
            if (!departments.TryGetValue(t.DepartmentName, out var dept))
                throw new InvalidOperationException($"Seed dependency missing: department '{t.DepartmentName}'.");

            var seq = 0;
            foreach (var brand in t.Brands)
            foreach (var color in t.Colors)
            foreach (var size in t.Sizes)
            {
                seq++;
                items.Add(new Item
                {
                    Sku          = $"{t.SkuPrefix}-{Abbrev(brand)}-{Abbrev(color)}-{NormalizeSize(size)}-{seq:D4}",
                    Name         = $"{brand} {t.Name} {color} {size}",
                    DepartmentId = dept.Id,
                    Category     = t.Category,
                    Brand        = brand,
                    Color        = color,
                    Size         = size,
                    CurrentPrice = t.BasePrice,
                    Cost         = Math.Round(t.BasePrice * t.CostRatio, 2),
                    Status       = ItemStatus.Active
                });
            }
        }

        return items;
    }

    private static string Abbrev(string s)
    {
        var clean = new string(s.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        return clean.Length <= 4 ? clean : clean[..4];
    }

    private static string NormalizeSize(string s) =>
        new string(s.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToUpperInvariant();
}
