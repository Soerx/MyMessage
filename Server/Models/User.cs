using Microsoft.EntityFrameworkCore;

namespace Server.Models;

[Index(nameof(Username), IsUnique = true)]
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public Rank Rank { get; set; } = null!;
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateTime Birthdate { get; set; }
    public byte[]? Image { get; set; }
    public string? Status { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastActivity {  get; set; }
    public List<Chat>? Chats { get; set; }
}

public enum Gender
{
    Unspecified,
    Male,
    Female
}