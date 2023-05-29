using System.Collections.ObjectModel;

namespace Client.Models;

public class Data
{
    public ObservableCollection<User> Users { get; set; } = null!;
    public ObservableCollection<Message> Messages { get; set; } = null!;
    public ObservableCollection<MessageContent> MessagesContents { get; set; } = null!;
}