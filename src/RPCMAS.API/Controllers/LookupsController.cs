using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RPCMAS.Core.Enums;
using RPCMAS.Infrastructure.Data;

namespace RPCMAS.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/lookups")]
public class LookupsController : ControllerBase
{
    private readonly AppDbContext _db;

    public LookupsController(AppDbContext db) => _db = db;

    [HttpGet("departments")]
    public async Task<IActionResult> Departments(CancellationToken ct)
        => Ok(await _db.Departments.AsNoTracking().Select(d => new { d.Id, d.Name }).ToListAsync(ct));

    [HttpGet("users")]
    public async Task<IActionResult> Users(CancellationToken ct)
        => Ok(await _db.Users.AsNoTracking()
            .Select(u => new { u.Id, u.Name, Role = u.Role.ToString(), u.DepartmentId })
            .ToListAsync(ct));

    [HttpGet("change-types")]
    public IActionResult ChangeTypes()
        => Ok(Enum.GetValues<ChangeType>().Select(t => new { Id = (int)t, Name = t.ToString() }));

    [HttpGet("statuses")]
    public IActionResult Statuses()
        => Ok(Enum.GetValues<RequestStatus>().Select(s => new { Id = (int)s, Name = s.ToString() }));
}
