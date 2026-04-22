using RPCMAS.Core.Enums;

namespace RPCMAS.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public UserRole Role { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
}
