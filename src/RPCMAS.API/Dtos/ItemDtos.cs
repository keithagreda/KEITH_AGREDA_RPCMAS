using RPCMAS.Core.Entities;
using RPCMAS.Core.Enums;

namespace RPCMAS.API.Dtos;

public record ItemDto(
    int Id,
    string Sku,
    string Name,
    int DepartmentId,
    string DepartmentName,
    string Category,
    string Brand,
    string? Color,
    string? Size,
    decimal CurrentPrice,
    decimal Cost,
    ItemStatus Status,
    string RowVersion)
{
    public static ItemDto From(Item i) => new(
        i.Id, i.Sku, i.Name,
        i.DepartmentId, i.Department?.Name ?? "",
        i.Category, i.Brand, i.Color, i.Size,
        i.CurrentPrice, i.Cost, i.Status,
        Convert.ToBase64String(i.RowVersion));
}
