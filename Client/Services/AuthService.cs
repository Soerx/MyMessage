using Client.Args;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;
using System;
using Client.Models;
using Client.Tools;
using System.Threading;

namespace Client.Services
{
    public class AuthService : IDisposable
    {
        private bool _disposed;
        private HubConnection _connection = null!;
        private string? _token;
        private bool _isResponseReceived;
        private bool _isResponseSuccess;
        private string? _exception;

        public AuthService()
        {
            SetUpService();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                _connection?.StopAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _connection?.DisposeAsync();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public bool Register(RegisterArgs args, out string? token, out string? exception)
        {
            exception = null;

            try
            {
                _connection.InvokeAsync("Register", args).Wait();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return WaitResponse(out token, out exception);
        }

        public bool Auth(AuthArgs args, out string? token, out string? exception)
        {
            try
            {
                _connection.InvokeAsync("Auth", args).Wait();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return WaitResponse(out token, out exception);
        }

        private async void SetUpService()
        {
            string ipAddress = ServerConnectionTools.IPAddress;
            int port = ServerConnectionTools.PORT;

            _connection = new HubConnectionBuilder().WithUrl($"http://{ipAddress}:{port}/AuthHub").Build();
            _connection.On<string, User>("ReceiveToken", ReceiveToken);
            _connection.On<string?>("ReceiveBadAuthResponse", ReceiveBadAuthResponse);

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool WaitResponse(out string? token, out string? exception)
        {
            exception = null;
            token = null;
            int reconnectAttemptsLimit = 60;
            int waitingBetweenAttemptsMilliseconds = 1000;

            for (int i = 0; i < reconnectAttemptsLimit; i++)
            {
                if (_isResponseReceived)
                {
                    exception = _exception;
                    token = _token;
                    return _isResponseSuccess;
                }

                Thread.Sleep(waitingBetweenAttemptsMilliseconds);
            }

            MessageBox.Show("Не удается получить ответ от сервера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        private void ReceiveBadAuthResponse(string? exception = null)
        {
            _exception = exception;
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