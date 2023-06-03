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
        public event Action<string>? ErrorMessageReceived;
        public event Action? ConfirmationReceived;
        public event Action<MessageViewModel>? MessageEditButtonClicked;

        private bool _disposed;
        private string? _token;
        private HubConnection _connection = null!;
        private ObservableCollection<User> _users = null!;

        public ObservableCollection<User> Users
        {
            get => _users ??= new();
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

        public async ValueTask UpdateMessage(Message message)
        {
            await TryExecuteAsync(async () => await _connection.InvokeAsync("UpdateMessage", message));
        }

        public async ValueTask UpdateCurrentUserData(User updatedUser)
        {
            await TryExecuteAsync(async () => await _connection.InvokeAsync("UpdateCurrentUser", updatedUser));
        }

        public async ValueTask UpdateCurrentUserPassword(string currentPassword, string newPassword)
        {
            await TryExecuteAsync(async () => await _connection.InvokeAsync("UpdateCurrentUserPassword", currentPassword, newPassword));
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
            _connection.On<User>("ReceiveUserData", ReceiveUserData);
            _connection.On<string>("ReceiveErrorMessage", ReceiveErrorMessage);
            _connection.On("ReceiveConfirmation", ReceiveConfirmation);
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

            Users = data.Users;

            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                foreach (Message message in data.Messages)
                {
                    User interlocuter;

                    if (message.ReceiverUsername == App.Instance.CurrentUser.Username)
                        interlocuter = Users.Single(u => u.Username == message.SenderUsername);
                    else
                        interlocuter = Users.Single(u => u.Username == message.ReceiverUsername);

                    interlocuter.Messages.Add(new(message, NotifyMessageEditButtonClicked));

                    if (message.IsReceived is false && message.ReceiverUsername == App.Instance.CurrentUser.Username)
                        await TryExecuteAsync(async () => await UpdateMessage(message));
                }
            });
        }

        private async void ReceiveMessageData(Message message)
        {
            if (message is null)
                return;

            User interlocuter;

            if (message.ReceiverUsername == App.Instance.CurrentUser.Username)
                interlocuter = Users.Single(u => u.Username == message.SenderUsername);
            else
                interlocuter = Users.Single(u => u.Username == message.ReceiverUsername);

            MessageViewModel? foundMessageViewModel = interlocuter.Messages.SingleOrDefault(mVM => mVM.Message.Id == message.Id);

            if (foundMessageViewModel is not null)
            {
                foundMessageViewModel.Message.IsReceived = message.IsReceived;
                foundMessageViewModel.Message.IsRead = message.IsRead;
                foundMessageViewModel.Message.IsEdited = message.IsEdited;
                foundMessageViewModel.Message.IsDeleted = message.IsDeleted;
                foundMessageViewModel.Message.Content.Text = message.Content.Text;
                foundMessageViewModel.Message.Content.Images = message.Content.Images;
            }
            else
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    MessageViewModel newMessageViewModel = new(message, NotifyMessageEditButtonClicked);
                    interlocuter.Messages.Add(newMessageViewModel);
                    MessageReceived?.Invoke(newMessageViewModel);

                    if (message.IsReceived == false && message.ReceiverUsername == App.Instance.CurrentUser.Username)
                        await TryExecuteAsync(async () => await UpdateMessage(message));
                });
            }
        }

        private async void ReceiveUserData(User user)
        {
            Users ??= new();

            if (user is null)
                return;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                User? foundUser = Users.SingleOrDefault(u => u.Username == user.Username);

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

        private void ReceiveConfirmation()
        {
            ConfirmationReceived?.Invoke();
        }

        private void NotifyMessageEditButtonClicked(MessageViewModel messageViewModel)
        {
            MessageEditButtonClicked?.Invoke(messageViewModel);
        }
    }
}