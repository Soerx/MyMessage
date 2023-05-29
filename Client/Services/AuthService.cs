using Client.Args;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;
using System;
using Client.Models;
using Client.Tools;
using System.Threading.Tasks;

namespace Client.Services
{
    public class AuthService : IDisposable
    {
        private bool _disposed;
        private HubConnection _connection = null!;
        private string? _token;
        private bool _isResponseReceived;
        private bool _isResponseSuccess;
        private string? _error;

        public AuthService()
        {
            SetUpService();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _connection?.StopAsync();
            _connection?.DisposeAsync();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public async Task<(bool, string?, string?)> Register(RegisterArgs args)
        {
            await TryExecuteAsync(() => _connection.InvokeAsync("Register", args));
            return (await WaitResponse(), _token, _error);
        }

        public async Task<(bool, string?, string?)> Auth(AuthArgs args)
        {
            await TryExecuteAsync(() => _connection.InvokeAsync("Auth", args));
            return (await WaitResponse(), _token, _error);
        }

        private async void SetUpService()
        {
            string ipAddress = ServerConnectionTools.IPAddress;
            int port = ServerConnectionTools.PORT;
            _connection = new HubConnectionBuilder().WithUrl($"http://{ipAddress}:{port}/AuthHub").Build();
            _connection.On<string, User>("ReceiveToken", ReceiveToken);
            _connection.On<string>("ReceiveErrorMessage", ReceiveErrorMessage);
            await TryExecuteAsync(() => _connection.StartAsync());
        }

        private async Task TryExecuteAsync(Func<Task> execute)
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

        private async Task<bool> WaitResponse()
        {
            int reconnectAttemptsLimit = 60;
            int waitingBetweenAttemptsMilliseconds = 1000;

            for (int i = 0; i < reconnectAttemptsLimit; i++)
            {
                if (_isResponseReceived)
                {
                    return _isResponseSuccess;
                }

                await Task.Delay(waitingBetweenAttemptsMilliseconds);
            }

            MessageBox.Show("Не удается получить ответ от сервера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        private void ReceiveErrorMessage(string errorMessage)
        {
            _error = errorMessage;
            _isResponseReceived = true;
        }

        private void ReceiveToken(string token, User user)
        {
            App.Instance.CurrentUser = user;
            _token = token;
            _isResponseReceived = true;
            _isResponseSuccess = true;
        }
    }
}