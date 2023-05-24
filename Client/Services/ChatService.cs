using Client.Models;
using Client.Tools;
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
        public event Action<Message>? MessageReceived;

        private bool _disposed;
        private HubConnection _connection = null!;
        private string? _token;
        private ObservableCollection<Chat> _chats = null!;
        private ObservableCollection<Message> _messages = null!;
        private ObservableCollection<User> _users = null!;

        public ObservableCollection<Chat> Chats
        {
            get => _chats;
            private set
            {
                _chats = value;
                RaisePropertyChanged(nameof(Chats));
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

        public ObservableCollection<User> Users
        {
            get => _users;
            private set
            {
                _users = value;
                RaisePropertyChanged(nameof(Users));
            }
        }

        public ChatService(string token)
        {
            _token = token;
            SetUpService();
            Chats = new ObservableCollection<Chat>();
            Users = new ObservableCollection<User>();
            Messages = new ObservableCollection<Message>();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _connection?.StopAsync();
            _connection?.DisposeAsync();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public async Task SendMessage(string message, User to)
        {
            try
            {
                await _connection.InvokeAsync("SendMessage", message, to);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdateMessage(Message message)
        {
            try
            {
                await _connection.InvokeAsync("UpdateMessage", message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdateChat(Chat chat)
        {
            try
            {
                await _connection.InvokeAsync("UpdateChat", chat);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdateCurrentUserData()
        {
            try
            {
                await _connection.InvokeAsync("UpdateCurrentUser", App.Instance.CurrentUser);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            _connection.On<Message>("ReceiveMessageData", ReceiveMessageData);
            _connection.On<Chat>("ReceiveChatData", ReceiveChatData);
            _connection.On<User>("ReceiveUserData", ReceiveUserData);

            try
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("SyncData");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SyncData(Data data)
        {
            Users = new ObservableCollection<User>(data.Users);
            Messages = new ObservableCollection<Message>(data.Messages);
            Chats = new ObservableCollection<Chat>(data.Chats);
        }

        private void ReceiveMessageData(Message message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Messages.Any(m => m.Id == message.Id))
                {
                    var foundMsg = Messages.First(m => m.Id == message.Id);
                    foundMsg.IsReceived = message.IsReceived;
                    foundMsg.IsRead = message.IsRead;
                    foundMsg.IsEdited = message.IsEdited;
                    foundMsg.IsDeleted = message.IsDeleted;
                    foundMsg.Content = message.Content;
                }
                else
                {
                    Messages.Add(message);
                    MessageReceived?.Invoke(message);
                }
            });
        }

        private void ReceiveChatData(Chat chat)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Chats.Any(c => c.Id == chat.Id))
                {
                    var foundChat = Chats.First(c => c.Id == chat.Id);
                    foundChat.Name = chat.Name;
                    foundChat.Owner = chat.Owner;
                }
                else
                {
                    Chats.Add(chat);
                }
            });
        }

        private void ReceiveUserData(User user)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Users.Any(u => u.Id == user.Id))
                {
                    var foundUser = Users.First(u => u.Id == user.Id);
                    foundUser.Rank = user.Rank;
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
                    App.Instance.CurrentUser.Rank = user.Rank;
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
    }
}