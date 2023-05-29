namespace Server.Entities;

public class MessageContentEntity
{
    public int Id { get; set; }
    public string? Text { get; set; }
    public List<ImageEntity>? Images { get; set; }
}

