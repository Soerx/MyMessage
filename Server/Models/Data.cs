namespace Server.Models;

public class Data
{
    public IEnumerable<User> Users { get; set; } = null!;
    public IEnumerable<Chat> Chats { get; set; } = null!;
}