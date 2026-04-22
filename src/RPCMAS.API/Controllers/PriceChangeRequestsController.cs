using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RPCMAS.API.Dtos;
using RPCMAS.Core.Common;
using RPCMAS.Core.Enums;
using RPCMAS.Infrastructure.Services;
using RPCMAS.Infrastructure.Services.Models;

namespace RPCMAS.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/price-change-requests")]
public class PriceChangeRequestsController : ControllerBase
{
    private readonly IPriceChangeRequestService _service;

    public PriceChangeRequestsController(IPriceChangeRequestService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponse<PriceChangeRequestSummaryDto>>> List(
        [FromQuery] string? requestNumber,
        [FromQuery] RequestStatus? status,
        [FromQuery] int? departmentId,
        [FromQuery] ChangeType? changeType,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        var query = new PriceChangeRequestQuery
        {
            RequestNumber = requestNumber,
            Status = status,
            DepartmentId = departmentId,
            ChangeType = changeType,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page,
            PageSize = pageSize
        };
        var result = await _service.ListAsync(query, ct);
        var dto = new PagedResponse<PriceChangeRequestSummaryDto>(
            result.Items.Select(PriceChangeRequestSummaryDto.From).ToList(),
            result.Page, result.PageSize, result.TotalCount, result.TotalPages);
        return Ok(dto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PriceChangeRequestDto>> Get(int id, CancellationToken ct = default)
    {
        var r = await _service.GetByIdAsync(id, ct);
        return Ok(PriceChangeRequestDto.From(r));
    }

    [HttpPost]
    public async Task<ActionResult<PriceChangeRequestDto>> Create([FromBody] CreateRequestDto dto, CancellationToken ct = default)
    {
        var input = new CreateRequestInput
        {
            DepartmentId = dto.DepartmentId,
            ChangeType = dto.ChangeType,
            Reason = dto.Reason,
            Details = dto.Details.Select(d => new RequestDetailInput
            {
                ItemId = d.ItemId,
                ProposedNewPrice = d.ProposedNewPrice,
                EffectiveDate = d.EffectiveDate,
                Remarks = d.Remarks
            }).ToList()
        };
        var created = await _service.CreateAsync(input, ct);
        var result = PriceChangeRequestDto.From(created);
        return CreatedAtAction(nameof(Get), new { id = created.Id, version = "1.0" }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PriceChangeRequestDto>> Update(int id, [FromBody] UpdateRequestDto dto, CancellationToken ct = default)
    {
        var input = new UpdateRequestInput
        {
            ChangeType = dto.ChangeType,
            Reason = dto.Reason,
            RowVersion = Convert.FromBase64String(dto.RowVersion),
            Details = dto.Details.Select(d => new RequestDetailInput
            {
                ItemId = d.ItemId,
                ProposedNewPrice = d.ProposedNewPrice,
                EffectiveDate = d.EffectiveDate,
                Remarks = d.Remarks
            }).ToList()
        };
        var updated = await _service.UpdateAsync(id, input, ct);
        return Ok(PriceChangeRequestDto.From(updated));
    }

    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id, [FromBody] WorkflowActionDto dto, CancellationToken ct = default)
    {
        await _service.SubmitAsync(id, Convert.FromBase64String(dto.RowVersion), ct);
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] WorkflowActionDto dto, CancellationToken ct = default)
    {
        await _service.ApproveAsync(id, Convert.FromBase64String(dto.RowVersion), ct);
        return NoContent();
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectActionDto dto, CancellationToken ct = default)
    {
        await _service.RejectAsync(id, Convert.FromBase64String(dto.RowVersion), dto.Reason, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/apply")]
    public async Task<IActionResult> Apply(int id, [FromBody] WorkflowActionDto dto, CancellationToken ct = default)
    {
        await _service.ApplyAsync(id, Convert.FromBase64String(dto.RowVersion), ct);
        return NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] WorkflowActionDto dto, CancellationToken ct = default)
    {
        await _service.CancelAsync(id, Convert.FromBase64String(dto.RowVersion), ct);
        return NoContent();
    }
}
