using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Entities;

[Index(nameof(Username), IsUnique = true)]
public class UserEntity
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public byte[] Password { get; set; } = null!;
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateTime Birthdate { get; set; }
    public byte[]? Image { get; set; }
    public string? Status { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastActivity {  get; set; } = DateTime.Now;
}