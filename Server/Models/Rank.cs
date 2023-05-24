using Microsoft.EntityFrameworkCore;

namespace Server.Models;

[Index(nameof(Name), IsUnique = true)]
public class Rank
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Priority { get; set; }
    public string? Description { get; set; }
}
