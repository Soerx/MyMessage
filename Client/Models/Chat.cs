using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Client.Models;

public class Chat : BindableBase
{
    private string? _name;
    private ObservableCollection<User> _users = null!;
    private ObservableCollection<Message>? _messages;

    public int Id { get; set; }

    public string? Name
    {
        get => _name;
        set
        {
            _name = value;
            RaisePropertyChanged(nameof(Name));
        }
    }

    public string DisplayName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                if (App.Instance.CurrentUser is not null && Users is not null)
                {
                    int userIndex = 0;
                    string displayName = Users[userIndex].Id != App.Instance.CurrentUser.Id ? Users[userIndex].ToString() : Users[++userIndex].ToString();

                    for (int i = 0; i < Users.Count; i++)
                    {
                        if (Users[i].Id != App.Instance.CurrentUser.Id && Users[i].Id != Users[userIndex].Id)
                            displayName += ", " + Users[i];
                    }

                    return displayName;
                }

                return string.Empty;
            }
            else
            {
                return Name;
            }
        }
    }

    public ObservableCollection<User> Users
    {
        get => _users;
        set
        {
            _users = value;
            RaisePropertyChanged(nameof(Users));
            RaisePropertyChanged(nameof(Image));
        }
    }

    public ObservableCollection<Message>? Messages
    {
        get => _messages;
        set
        {
            _messages = value;
            RaisePropertyChanged(nameof(Messages));
            RaisePropertyChanged(nameof(LastMessage));
            RaisePropertyChanged(nameof(UnreadMessagesCount));
        }
    }

    public int UnreadMessagesCount
    {
        get
        {
            if (Messages is null || Messages.Count == 0)
                return 0;

            return Messages.Where(m => m.IsRead == false).Count();
        }
    }

    public Message? LastMessage
    {
        get
        {
            if (Messages is null ||  Messages.Count == 0)
                return null;

            return Messages.FirstOrDefault(m => m.Created == Messages.Max(m => m.Created));
        }
    }

    public BitmapSource? Image
    {
        get
        {
            int dialogUsersCount = 2;

            if (Users is not null && Users.Count == dialogUsersCount && App.Instance.CurrentUser is not null)
            {
                User? receiver = Users.SingleOrDefault(u => u.Id != App.Instance.CurrentUser.Id);

                if (receiver is null)
                    return null;

                return receiver.BitmapImage;
            }

            return null;
        }
    }
}
