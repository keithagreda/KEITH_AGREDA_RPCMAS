using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RPCMAS.API.Dtos;
using RPCMAS.Core.Common;
using RPCMAS.Infrastructure.Services;

namespace RPCMAS.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/items")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _service;

    public ItemsController(IItemService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ItemDto>>> List(
        [FromQuery] string? search,
        [FromQuery] int? departmentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        var result = await _service.ListAsync(new ItemQuery { Search = search, DepartmentId = departmentId, Page = page, PageSize = pageSize }, ct);
        var dto = new PagedResponse<ItemDto>(
            result.Items.Select(ItemDto.From).ToList(),
            result.Page, result.PageSize, result.TotalCount, result.TotalPages);
        return Ok(dto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ItemDto>> Get(int id, CancellationToken ct = default)
    {
        var item = await _service.GetByIdAsync(id, ct);
        return Ok(ItemDto.From(item));
    }
}
