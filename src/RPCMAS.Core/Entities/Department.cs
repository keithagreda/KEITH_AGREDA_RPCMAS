namespace RPCMAS.Core.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<Item> Items { get; set; } = new List<Item>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
