using Microsoft.EntityFrameworkCore;
using RPCMAS.Core.Enums;
using RPCMAS.Core.Interfaces;
using RPCMAS.Infrastructure.Data;

namespace RPCMAS.API.Auth;

public class MockCurrentUserService : ICurrentUserService
{
    public const string HeaderName = "X-User-Id";

    private readonly Lazy<UserSnapshot> _snapshot;

    public MockCurrentUserService(IHttpContextAccessor accessor, AppDbContext db)
    {
        _snapshot = new Lazy<UserSnapshot>(() => Resolve(accessor, db));
    }

    public int UserId => _snapshot.Value.Id;
    public string UserName => _snapshot.Value.Name;
    public UserRole Role => _snapshot.Value.Role;
    public int? DepartmentId => _snapshot.Value.DepartmentId;

    private static UserSnapshot Resolve(IHttpContextAccessor accessor, AppDbContext db)
    {
        var headerVal = accessor.HttpContext?.Request.Headers[HeaderName].FirstOrDefault();
        int.TryParse(headerVal, out var userId);

        var user = userId > 0
            ? db.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId)
            : null;

        user ??= db.Users.AsNoTracking().OrderBy(u => u.Id).First();

        return new UserSnapshot(user.Id, user.Name, user.Role, user.DepartmentId);
    }

    private record UserSnapshot(int Id, string Name, UserRole Role, int? DepartmentId);
}
