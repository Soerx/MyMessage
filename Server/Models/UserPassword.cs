using Microsoft.EntityFrameworkCore;

namespace Server.Models;

public class UserPassword
{
    public int Id { get; set; }
    public User User { get; set; } = null!;
    public byte[] Password { get; set; } = null!;
}
