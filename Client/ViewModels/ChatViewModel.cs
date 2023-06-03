using Client.Commands;
using Client.Models;
using Client.Services;
using Client.Stores;
using Client.Tools;
using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels;

public class ChatViewModel : BindableBase, IDisposable
{
    private bool _disposed;
    private readonly NavigationStore _navigationStore;
    private BindableBase _previewViewModel;
    private string? _sendingMessage;
    private bool _isEditingMessage;
    private MessageViewModel _editingMessage = null!;
    private ObservableCollection<ImageModel> _attachedImages = null!;

    public ChatService ChatService { get; }

    public ObservableCollection<ImageModel> AttachedImages
    {
        get => _attachedImages ??= new();
        private set
        {
            _attachedImages = value;
            RaisePropertyChanged(nameof(AttachedImages));
            RaisePropertyChanged(nameof(AttachedImagesVisibility));
        }
    }

    public Visibility AttachedImagesVisibility => AttachedImages.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public User Interlocutor { get; }

    public string? SendingMessage
    {
        get => _sendingMessage;
        set
        {
            _sendingMessage = value;
            RaisePropertyChanged(nameof(SendingMessage));
        }
    }

    public bool IsEditingMessage
    {
        get => _isEditingMessage;
        set
        {
            _isEditingMessage = value;
            RaisePropertyChanged(nameof(IsEditingMessage));
            RaisePropertyChanged(nameof(EditButtonVisibility));
            RaisePropertyChanged(nameof(SendButtonVisibility));
        }
    }

    public Visibility EditButtonVisibility => IsEditingMessage ? Visibility.Visible : Visibility.Collapsed;
    public Visibility SendButtonVisibility => IsEditingMessage ? Visibility.Collapsed : Visibility.Visible;

    public ICommand SendMessageCommand { get; }
    public ICommand EditMessageCommand { get; }
    public ICommand CancelEditMessageCommand { get; }
    public ICommand GoProfileCommand { get; }
    public ICommand GoBackCommand { get; }
    public ICommand AttachImageCommand { get; }
    public ICommand DetachImageCommand { get; }

    public ChatViewModel(NavigationStore navigationStore, ChatService chatService, User interlocutor, BindableBase previewViewModel)
    {
        ChatService = chatService;
        _navigationStore = navigationStore;
        _previewViewModel = previewViewModel;
        Interlocutor = interlocutor;
        SendMessageCommand = new RelayCommand(SendMessage);
        GoProfileCommand = new RelayCommand(GoToUser);
        EditMessageCommand = new RelayCommand(EditMessage);
        CancelEditMessageCommand = new RelayCommand(CancelEditMessage);
        GoBackCommand = new RelayCommand(GoBack);
        AttachImageCommand = new AsyncRelayCommand(AttachImage);
        DetachImageCommand = new RelayCommand(DetachImage);
        ChatService.MessageEditButtonClicked += StartEditMessage;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        ChatService.MessageEditButtonClicked -= StartEditMessage;
        GC.SuppressFinalize(this);
    }

    private void GoToUser(object parameter)
    {
        _navigationStore.CurrentViewModel = new ProfileViewModel(Interlocutor, _navigationStore, this);
    }

    private void GoBack(object parameter)
    {
        _navigationStore.CurrentViewModel = _previewViewModel;
    }

    private async void SendMessage(object parameter)
    {
        bool textExist = false;
        bool imagesExists = false;

        if (string.IsNullOrWhiteSpace(SendingMessage) == false)
            textExist = true;

        if (AttachedImages.Count > 0)
            imagesExists = true;

        if (textExist || imagesExists)
        {
            await ChatService.SendMessage(new MessageContent() { Text = SendingMessage, Images = AttachedImages }, Interlocutor.Username);
            AttachedImages.Clear();
            SendingMessage = string.Empty;
        }
    }

    private async void EditMessage(object parameter)
    {
        bool isTextUpdated = false;
        bool isImagesUpdated = false;

        if (IsEditingMessage == false || _editingMessage is null)
            return;

        if (string.IsNullOrWhiteSpace(SendingMessage) && AttachedImages.Count == 0)
            return;

        if (SendingMessage != _editingMessage.Message.Content.Text)
            isTextUpdated = true;

        if (_editingMessage.Message.Content.Images is null || AttachedImages.SequenceEqual(_editingMessage.Message.Content.Images) is false)
            isImagesUpdated = true;

        if (isTextUpdated || isImagesUpdated)
        {
            _editingMessage.Message.Content.Text = SendingMessage;
            _editingMessage.Message.Content.Images = new(AttachedImages);
            _editingMessage.Message.IsEdited = true;
            await ChatService.UpdateMessage(_editingMessage.Message);
        }

        AttachedImages.Clear();
        SendingMessage = string.Empty;
        IsEditingMessage = false;
    }

    private void CancelEditMessage(object parameter)
    {
        AttachedImages.Clear();
        SendingMessage = string.Empty;
        IsEditingMessage = false;
    }

    private async ValueTask AttachImage(object parameter)
    {
        OpenFileDialog dialog = new OpenFileDialog();
        dialog.Filter = "Файлы изображений (*.bmp, *.jpg, *.jpeg, *.png)|*.bmp;*.jpg;*.jpeg;*.png";

        if (dialog.ShowDialog() is false)
            return;

        string filename = dialog.SafeFileName;
        string bmpFileExtension = "bmp";
        string jpgFileExtension = "jpg";
        string jpegFileExtension = "jpeg";
        string pngFileExtension = "png";
        string imageWord = "image";
        string currentFileExtension;

        if (filename.EndsWith(bmpFileExtension))
            currentFileExtension = bmpFileExtension;
        else if (filename.EndsWith(jpgFileExtension))
            currentFileExtension = jpegFileExtension;
        else if (filename.EndsWith(jpegFileExtension))
            currentFileExtension = jpegFileExtension;
        else
            currentFileExtension = pngFileExtension;

        using MultipartFormDataContent multipartFormContent = new();
        using HttpClient httpClient = new();
        using StreamContent fileStreamContent = new(File.OpenRead(dialog.FileName));
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue($"{imageWord}/{currentFileExtension}");
        multipartFormContent.Add(fileStreamContent, "uploadedFile", filename);
        HttpResponseMessage response = await httpClient.PostAsync($"http://{ServerConnectionTools.IPAddress}:{ServerConnectionTools.PORT}/UploadImage", multipartFormContent);

        if (response.IsSuccessStatusCode)
        {
            ImageModel? image = await response.Content.ReadFromJsonAsync<ImageModel>();

            if (image is not null)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AttachedImages.Add(image);
                });
        }
        else
        {
            var content = await response.Content.ReadAsStringAsync();
            MessageBox.Show($"Ошибка загрузки картинки: {content}");
        }
    }

    public void DetachImage(object parameter)
    {
        if (parameter is ImageModel image)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AttachedImages.Remove(image);
            });
        }
    }

    public void StartEditMessage(MessageViewModel editingMessage)
    {
        if (editingMessage.Message.SenderUsername != App.Instance.CurrentUser.Username)
            return;

        _editingMessage = editingMessage;
        AttachedImages = editingMessage.Message.Content.Images is null ? AttachedImages : new(editingMessage.Message.Content.Images);
        SendingMessage = editingMessage.Message.Content.Text;
        IsEditingMessage = true;
    }

    ~ChatViewModel()
    {
        Dispose();
    }
}