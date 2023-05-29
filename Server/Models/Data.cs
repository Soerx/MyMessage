namespace Server.Models;

public class Data
{
    public IEnumerable<User> Users { get; set; } = null!;
    public IEnumerable<Message> Messages { get; set; } = null!;
    public IEnumerable<MessageContent> MessagesContents { get; set; } = null!;
}