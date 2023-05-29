namespace Server.Entities;

public class MessageEntity
{
    public int Id { get; set; }
    public UserEntity Sender { get; set; } = null!;
    public UserEntity Receiver { get; set; } = null!;
    public MessageContentEntity Content { get; set; } = null!;
    public DateTime Created { get; set; } = DateTime.Now;
    public bool IsEdited { get; set; }
    public bool IsReceived { get; set; }
    public bool IsRead { get; set; }
    public bool IsDeleted { get; set; }
}