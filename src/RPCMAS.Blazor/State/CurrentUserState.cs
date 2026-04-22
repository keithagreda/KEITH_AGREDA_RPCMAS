using RPCMAS.Blazor.Api;

namespace RPCMAS.Blazor.State;

public class CurrentUserState
{
    public int? UserId { get; private set; }
    public string? UserName { get; private set; }
    public string? Role { get; private set; }
    public int? DepartmentId { get; private set; }

    public event Action? Changed;

    public void Set(UserLookup user)
    {
        UserId = user.Id;
        UserName = user.Name;
        Role = user.Role;
        DepartmentId = user.DepartmentId;
        Changed?.Invoke();
    }
}
