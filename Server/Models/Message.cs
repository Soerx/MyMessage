namespace Server.Models;

public class Message
{
    public int Id { get; set; }
    public string SenderUsername { get; set; } = null!;
    public string ReceiverUsername { get; set; } = null!;
    public int ContentId { get; set; }
    public DateTime Created { get; set; }
    public bool IsEdited { get; set; }
    public bool IsReceived { get; set; }
    public bool IsRead { get; set; }
    public bool IsDeleted { get; set; }
}