using Client.Controls;
using Client.Models;
using Client.Tools;
using Client.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Client.Services
{
    public class ChatService : BindableBase, IDisposable
    {
        public event Action<MessageViewModel>? MessageReceived;
        public event Action<Message>? MessageUpdated;
        public event Action<string>? ErrorMessageReceived;
        public event Action<MessageViewModel>? MessageEditButtonClicked;

        private bool _disposed;
        private HubConnection _connection = null!;
        private string? _token;
        private ObservableCollection<User> _users = null!;
        private ObservableCollection<Message> _messages = null!;
        private ObservableCollection<MessageContent> _messagesContents = null!;
        private ObservableCollection<MessageViewModel> _messagesViewModels = null!;
        private ObservableCollection<MessageControl> _messagesControls = null!;

        public ObservableCollection<User> Users
        {
            get => _users;
            private set
            {
                _users = value;
                RaisePropertyChanged(nameof(Users));
            }
        }

        public ObservableCollection<Message> Messages
        {
            get => _messages;
            private set
            {
                _messages = value;
                RaisePropertyChanged(nameof(Messages));
            }
        }

        public ObservableCollection<MessageContent> MessagesContents
        {
            get => _messagesContents;
            private set
            {
                _messagesContents = value;
                RaisePropertyChanged(nameof(MessagesContents));
            }
        }

        public ObservableCollection<MessageViewModel> MessagesViewModels
        {
            get => _messagesViewModels;
            private set
            {
                _messagesViewModels = value;
                RaisePropertyChanged(nameof(MessagesViewModels));
            }
        }

        public ObservableCollection<MessageControl> MessagesControls
        {
            get => _messagesControls;
            private set
            {
                _messagesControls = value;
                RaisePropertyChanged(nameof(MessagesControls));
            }
        }

        public ChatService(string token)
        {
            _token = token;
            SetUpService();
            Users ??= new();
            Messages ??= new();
            MessagesContents ??= new();
            MessagesViewModels ??= new();
            MessagesControls ??= new();
        }

        public async void Dispose()
        {
            if (_disposed)
                return;

            await _connection.StopAsync();
            _connection?.DisposeAsync();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public async ValueTask SendMessage(MessageContent messageContent, string receiverUsername)
        {
            await TryExecuteAsync(async () => await _connection.InvokeAsync("SendMessage", messageContent, receiverUsername));
        }

        public async ValueTask UpdateMessage(Message message, MessageContent messageContent)
        {
            await TryExecuteAsync(async () => await _connection.InvokeAsync("UpdateMessage", message, messageContent));
        }

        public async ValueTask UpdateCurrentUserData(User updatedUser)
        {
            await TryExecuteAsync(async () => await _connection.InvokeAsync("UpdateCurrentUser", updatedUser));
        }

        private async void SetUpService()
        {
            string ipAddress = ServerConnectionTools.IPAddress;
            int port = ServerConnectionTools.PORT;

            _connection = new HubConnectionBuilder().WithUrl($"http://{ipAddress}:{port}/ChatHub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_token);
            }).WithAutomaticReconnect().Build();

            _connection.On<Data>("SyncData", SyncData);
            _connection.On<Message, MessageContent>("ReceiveMessageData", ReceiveMessageData);
            _connection.On<User>("ReceiveUserData", ReceiveUserData);
            _connection.On<string>("ReceiveErrorMessage", ReceiveErrorMessage);
            await TryExecuteAsync(async () => await _connection.StartAsync());
            await TryExecuteAsync(async () => await _connection.InvokeAsync("SyncData"));
        }

        private async ValueTask TryExecuteAsync(Func<ValueTask> execute)
        {
            try
            {
                await execute();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SyncData(Data data)
        {
            if (data is null)
                return;

            Users = new ObservableCollection<User>(data.Users);
            Messages = new ObservableCollection<Message>();
            MessagesContents = new ObservableCollection<MessageContent>(data.MessagesContents);
            MessagesViewModels = new ObservableCollection<MessageViewModel>();
            MessagesControls = new ObservableCollection<MessageControl>();

            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                for (int i = 0; i < data.Messages.Count; i++)
                {
                    Messages.Add(new(data.Messages[i], NotifyMessageUpdated));
                    var messageContent = MessagesContents.First(mC => mC.Id == Messages[i].ContentId);
                    MessagesViewModels.Add(new MessageViewModel(Messages[i], messageContent, NotifyMessageEditButtonClicked));
                    MessagesControls.Add(new MessageControl() { DataContext = MessagesViewModels.Last() });

                    if (Messages[i].IsReceived == false && Messages[i].ReceiverUsername == App.Instance.CurrentUser.Username)
                        await TryExecuteAsync(async () => await UpdateMessage(Messages[i], messageContent));
                }
            });
        }

        private async void ReceiveMessageData(Message message, MessageContent messageContent)
        {
            if (message is null || messageContent is null)
                return;

            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                Message? foundMsg = Messages.SingleOrDefault(m => m.Id == message.Id);

                if (foundMsg is not null)
                {
                    MessageContent foundMsgContent = MessagesContents.First(mC => mC.Id == messageContent.Id);
                    foundMsg.IsReceived = message.IsReceived;
                    foundMsg.IsRead = message.IsRead;
                    foundMsg.IsEdited = message.IsEdited;
                    foundMsg.IsDeleted = message.IsDeleted;
                    foundMsgContent.Text = messageContent.Text;
                    foundMsgContent.Images = messageContent.Images;
                }
                else
                {
                    Messages.Add(new(message, NotifyMessageUpdated));
                    var updatedMessage = Messages.Last();
                    MessagesContents.Add(messageContent);
                    MessagesViewModels.Add(new MessageViewModel(updatedMessage, messageContent, NotifyMessageEditButtonClicked));
                    MessagesControls.Add(new MessageControl() { DataContext = MessagesViewModels.Last() });
                    MessageReceived?.Invoke(MessagesViewModels.Last());

                    if (updatedMessage.IsReceived == false && updatedMessage.ReceiverUsername == App.Instance.CurrentUser.Username)
                        await TryExecuteAsync(async () => await UpdateMessage(updatedMessage, messageContent));
                }
            });
        }

        private async void ReceiveUserData(User user)
        {
            Users ??= new();

            if (user is null)
                return;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                User? foundUser = Users.SingleOrDefault(u => u.Id == user.Id);

                if (foundUser is not null)
                {
                    foundUser.Firstname = user.Firstname;
                    foundUser.Lastname = user.Lastname;
                    foundUser.Gender = user.Gender;
                    foundUser.Birthdate = user.Birthdate;
                    foundUser.Image = user.Image;
                    foundUser.Status = user.Status;
                    foundUser.IsOnline = user.IsOnline;
                    foundUser.LastActivity = user.LastActivity;
                }
                else if (App.Instance.CurrentUser is not null && user.Id == App.Instance.CurrentUser.Id)
                {
                    App.Instance.CurrentUser.Firstname = user.Firstname;
                    App.Instance.CurrentUser.Lastname = user.Lastname;
                    App.Instance.CurrentUser.Gender = user.Gender;
                    App.Instance.CurrentUser.Birthdate = user.Birthdate;
                    App.Instance.CurrentUser.Image = user.Image;
                    App.Instance.CurrentUser.Status = user.Status;
                    App.Instance.CurrentUser.IsOnline = user.IsOnline;
                    App.Instance.CurrentUser.LastActivity = user.LastActivity;
                }
                else
                {
                    Users.Add(user);
                }
            });
        }

        private void ReceiveErrorMessage(string errorMessage)
        {
            ErrorMessageReceived?.Invoke(errorMessage);
        }

        private async void NotifyMessageUpdated(Message message)
        {
            await UpdateMessage(message, MessagesContents.First(mC => mC.Id == message.ContentId));
            MessageUpdated?.Invoke(message);
        }

        private void NotifyMessageEditButtonClicked(MessageViewModel messageViewModel)
        {
            MessageEditButtonClicked?.Invoke(messageViewModel);
        }
    }
}