namespace Server.Models;

public class MessageContent
{
    public int Id { get; set; }
    public string? Text { get; set; }
    public List<byte[]>? Images { get; set; }
}
