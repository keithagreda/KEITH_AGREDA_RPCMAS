using RPCMAS.Core.Enums;

namespace RPCMAS.Core.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }
    string UserName { get; }
    UserRole Role { get; }
    int? DepartmentId { get; }
}
