namespace Server.Models;

public class Chat
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public List<User> Users { get; set; } = null!;
    public List<Message>? Messages { get; set; }
}
