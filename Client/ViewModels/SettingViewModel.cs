using Client.Commands;
using Client.Services;
using MahApps.Metro.Controls;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System;
using Client.Models;
using Client.Tools;
using Microsoft.Win32;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;
using System.IO;
using System.Linq;

namespace Client.ViewModels
{
    public class SettingViewModel : BindableBase, IDisposable
    {
        private readonly ChatService _chatService;
        private bool _disposed;
        private string _firstname = null!;
        private string _lastname = null!;
        private List<GenderWrapper> _gendersWrappers;
        private GenderWrapper _genderWrapper = null!;
        private DateTime _birthdate;
        private string _newPassword = null!;
        private string _currentPassword = null!;
        private string _repeatPassword = null!;
        private PasswordBox _currentPasswordBox;
        private PasswordBox _passwordBox;
        private PasswordBox _repeatPasswordBox;
        private ToggleSwitch? _passwordVisibilitySwitcher;
        private Visibility _passwordVisibility;
        private Visibility _updateUserMessageVisibility;
        private Visibility _updateUserPasswordMessageVisibility;
        private string? _status;
        private ImageModel? _image;
        private string? _errorMessage;
        private bool _isUserUpdateAvailable;
        private bool _isUserPasswordUpdateAvailable;

        public bool IsUserUpdateAvailable
        {
            get => _isUserUpdateAvailable;
            set
            {
                _isUserUpdateAvailable = value;
                RaisePropertyChanged(nameof(IsUserUpdateAvailable));
                RaisePropertyChanged(nameof(UserUpdateProgressRingVisibility));
            }
        }

        public Visibility UserUpdateProgressRingVisibility => IsUserUpdateAvailable ? Visibility.Collapsed : Visibility.Visible;

        public bool IsUserPasswordUpdateAvailable
        {
            get => _isUserPasswordUpdateAvailable;
            set
            {
                _isUserPasswordUpdateAvailable = value;
                RaisePropertyChanged(nameof(IsUserPasswordUpdateAvailable));
                RaisePropertyChanged(nameof(UserPasswordUpdateProgressRingVisibility));
            }
        }

        public Visibility UserPasswordUpdateProgressRingVisibility => IsUserPasswordUpdateAvailable ? Visibility.Collapsed : Visibility.Visible;

        public string Firstname
        {
            get => _firstname ?? string.Empty;
            set
            {
                _firstname = value ?? string.Empty;
                RaisePropertyChanged(nameof(Firstname));
            }
        }

        public string Lastname
        {
            get => _lastname ?? string.Empty;
            set
            {
                _lastname = value ?? string.Empty;
                RaisePropertyChanged(nameof(Lastname));
            }
        }

        public List<GenderWrapper> GendersWrappers
        {
            get => _gendersWrappers;
            set
            {
                _gendersWrappers = value;
                RaisePropertyChanged(nameof(GendersWrappers));
            }
        }

        public GenderWrapper SelectedGenderWrapper
        {
            get => _genderWrapper;
            set
            {
                _genderWrapper = value;
                RaisePropertyChanged(nameof(SelectedGenderWrapper));
            }
        }

        public DateTime Birthdate
        {
            get => _birthdate;
            set
            {
                _birthdate = value;
                RaisePropertyChanged(nameof(Birthdate));
            }
        }

        public string? Status
        {
            get => _status ?? string.Empty;
            set
            {
                _status = value ?? string.Empty;
                RaisePropertyChanged(nameof(Status));
            }
        }

        public ImageModel? Image
        {
            get => _image;
            set
            {
                _image = value;
                RaisePropertyChanged(nameof(Image));
                RaisePropertyChanged(nameof(SelectImageButtonContent));
            }
        }

        public string Password
        {
            get => _newPassword ?? string.Empty;
            set
            {
                _newPassword = value ?? string.Empty;
                RaisePropertyChanged(nameof(Password));
            }
        }

        public string CurrentPassword
        {
            get => _currentPassword ?? string.Empty;
            set
            {
                _currentPassword = value ?? string.Empty;
                RaisePropertyChanged(nameof(CurrentPassword));
            }
        }

        public string RepeatPassword
        {
            get => _repeatPassword ?? string.Empty;
            set
            {
                _repeatPassword = value ?? string.Empty;
                RaisePropertyChanged(nameof(RepeatPassword));
            }
        }

        public PasswordBox CurrentPasswordField
        {
            get => _currentPasswordBox;
            private set
            {
                _currentPasswordBox = value;
                RaisePropertyChanged(nameof(CurrentPasswordField));
            }
        }

        public PasswordBox PasswordField
        {
            get => _passwordBox;
            private set
            {
                _passwordBox = value;
                RaisePropertyChanged(nameof(PasswordField));
            }
        }

        public PasswordBox RepeatPasswordField
        {
            get => _repeatPasswordBox;
            private set
            {
                _repeatPasswordBox = value;
                RaisePropertyChanged(nameof(RepeatPasswordField));
            }
        }

        public Visibility PasswordVisibility
        {
            get => _passwordVisibility;
            set
            {
                _passwordVisibility = value;
                RaisePropertyChanged(nameof(PasswordVisibility));
            }
        }

        public Visibility UpdateUserPasswordMessageVisibility
        {
            get => _updateUserPasswordMessageVisibility;
            set
            {
                _updateUserPasswordMessageVisibility = value;
                RaisePropertyChanged(nameof(UpdateUserPasswordMessageVisibility));
            }
        }

        public Visibility UpdateUserMessageVisibility
        {
            get => _updateUserMessageVisibility;
            set
            {
                _updateUserMessageVisibility = value;
                RaisePropertyChanged(nameof(UpdateUserMessageVisibility));
            }
        }

        public ToggleSwitch? PasswordVisibilitySwitcher
        {
            get => _passwordVisibilitySwitcher;
            private set
            {
                _passwordVisibilitySwitcher = value;
                RaisePropertyChanged(nameof(PasswordVisibilitySwitcher));
            }
        }

        public string? Message
        {
            get => _errorMessage ?? string.Empty;
            private set
            {
                _errorMessage = value;
                RaisePropertyChanged(nameof(Message));
            }
        }

        public string SelectImageButtonContent => Image is null ? "Выбрать изображение.." : Image.Name;

        public User User => App.Instance.CurrentUser;

        public ICommand UpdateCurrentUserCommand { get; }
        public ICommand UpdateCurrentUserPasswordCommand { get; }
        public ICommand AttachImageCommand { get; }

        public SettingViewModel(ChatService chatService)
        {
            _chatService = chatService;
            _passwordBox = new PasswordBox();
            _repeatPasswordBox = new PasswordBox();
            _currentPasswordBox = new PasswordBox();

            PasswordVisibilitySwitcher = new ToggleSwitch
            {
                OffContent = "Показать пароль",
                OnContent = "Скрыть пароль"
            };

            PasswordVisibilitySwitcher.Toggled += SwitchPasswordVisibility;
            _chatService.ErrorMessageReceived += ReceiveErrorMessage;
            _chatService.ConfirmationReceived += ReceiveConfirmation;
            UpdateCurrentUserCommand = new AsyncRelayCommand(UpdateCurrentUser);
            UpdateCurrentUserPasswordCommand = new AsyncRelayCommand(UpdateCurrentUserPassword);
            AttachImageCommand = new AsyncRelayCommand(AttachImage);
            PasswordVisibility = Visibility.Collapsed;
            UpdateUserMessageVisibility = Visibility.Collapsed;
            UpdateUserPasswordMessageVisibility = Visibility.Collapsed;
            _gendersWrappers = new List<GenderWrapper>();

            foreach (Gender gender in Enum.GetValues(typeof(Gender)))
            {
                _gendersWrappers.Add(new GenderWrapper(gender));
            }

            _isUserUpdateAvailable = true;
            _isUserPasswordUpdateAvailable = true;
            SelectedGenderWrapper = GendersWrappers.First(gW => gW.Gender == User.Gender);
            Firstname = User.Firstname;
            Lastname = User.Lastname;
            Image = User.Image;
            Status = User.Status;
            Birthdate = User.Birthdate;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Dispose(true);
            _chatService.ErrorMessageReceived -= ReceiveErrorMessage;
            _chatService.ConfirmationReceived -= ReceiveConfirmation;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (PasswordVisibilitySwitcher is not null)
                PasswordVisibilitySwitcher.Toggled -= SwitchPasswordVisibility;

            PasswordField = null!;
            PasswordVisibilitySwitcher = null;
            _disposed = true;
        }

        private async ValueTask UpdateCurrentUser(object parameter)
        {
            IsUserUpdateAvailable = false;
            Message = string.Empty;
            UpdateUserPasswordMessageVisibility = Visibility.Collapsed;
            UpdateUserMessageVisibility = Visibility.Visible;

            if (CheckFieldsValuesCorrect() == false)
                return;

            await _chatService.UpdateCurrentUserData(new()
            {
                Username = App.Instance.CurrentUser.Username,
                Firstname = Firstname,
                Lastname = Lastname,
                Gender = SelectedGenderWrapper.Gender,
                Birthdate = Birthdate,
                Status = Status,
                Image = Image
            });

            IsUserUpdateAvailable = true;
        }

        private async ValueTask UpdateCurrentUserPassword(object parameter)
        {
            Message = string.Empty;
            UpdateUserPasswordMessageVisibility = Visibility.Visible;
            UpdateUserMessageVisibility = Visibility.Collapsed;
            IsUserPasswordUpdateAvailable = false;

            if (PasswordVisibility is Visibility.Collapsed)
            {
                Password = PasswordField?.Password ?? string.Empty;
                RepeatPassword = RepeatPasswordField?.Password ?? string.Empty;
            }

            if (CheckFieldValueLengthCorrect(fieldName: "Пароль", Password, minLength: 8, maxLength: 80))
            {
                if (Password == RepeatPassword == false)
                {
                    Message = "• Пароли не совпадают.";
                    UpdateUserPasswordMessageVisibility = Visibility.Visible;
                    IsUserPasswordUpdateAvailable = true;
                    return;
                }

                await _chatService.UpdateCurrentUserPassword(CurrentPassword, Password);
            }

            IsUserPasswordUpdateAvailable = true;
        }

        private bool CheckFieldsValuesCorrect()
        {
            if (PasswordVisibility is Visibility.Collapsed)
            {
                Password = PasswordField?.Password ?? string.Empty;
                RepeatPassword = RepeatPasswordField?.Password ?? string.Empty;
            }

            if (CheckFieldValueLengthCorrect(fieldName: "Имя", Firstname, minLength: 2, maxLength: 120) &&
                CheckFieldValueLengthCorrect(fieldName: "Фамилия", Lastname, minLength: 2, maxLength: 120))
            {
                if (Password == RepeatPassword == false)
                {
                    Message = "• Пароли не совпадают.";
                    UpdateUserPasswordMessageVisibility = Visibility.Visible;
                    return false;
                }

                string minBirthdateDate = "01.01.1900";
                int minUserAge = 14;

                if (Birthdate.Date > DateTime.Parse(minBirthdateDate) &&
                    Birthdate.Date < DateTime.Now.AddYears(-minUserAge))
                {
                    return true;
                }
                else
                {
                    Message += $"\n• Некорректная дата рождения (Вы должны быть старше {minUserAge})";
                }
            }

            UpdateUserMessageVisibility = Visibility.Visible;
            return false;
        }

        private bool CheckFieldValueLengthCorrect(string fieldName, string fieldValue, int minLength, int maxLength)
        {
            if (fieldValue.Length > maxLength || fieldValue.Length < minLength)
            {
                Message += $"\n• Поле [{fieldName}] должно быть не менее {minLength} и не более {maxLength} символов";
                return false;
            }

            return true;
        }

        private void SwitchPasswordVisibility(object sender, RoutedEventArgs e)
        {
            if (PasswordField is null || RepeatPasswordField is null)
                return;

            if (PasswordVisibility is Visibility.Collapsed)
            {
                CurrentPassword = CurrentPasswordField.Password;
                Password = PasswordField.Password;
                RepeatPassword = RepeatPasswordField.Password;
                CurrentPasswordField.Visibility = Visibility.Collapsed;
                PasswordField.Visibility = Visibility.Collapsed;
                RepeatPasswordField.Visibility = Visibility.Collapsed;
                PasswordVisibility = Visibility.Visible;
            }
            else
            {
                CurrentPasswordField.Password = CurrentPassword;
                PasswordField.Password = Password;
                RepeatPasswordField.Password = RepeatPassword;
                CurrentPasswordField.Visibility = Visibility.Visible;
                PasswordField.Visibility = Visibility.Visible;
                RepeatPasswordField.Visibility = Visibility.Visible;
                PasswordVisibility = Visibility.Collapsed;
            }
        }

        private void ReceiveErrorMessage(string errorMessage)
        {
            Message = errorMessage;
        }

        private void ReceiveConfirmation()
        {
            Message = "Пароль успешно изменен";
        }

        private async ValueTask AttachImage(object parameter)
        {
            OpenFileDialog dialog = new()
            {
                Filter = "Файлы изображений (*.bmp, *.jpg, *.jpeg, *.png)|*.bmp;*.jpg;*.jpeg;*.png"
            };

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
                    Image = image;
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Ошибка загрузки картинки: {content}");
            }
        }

        public class GenderWrapper
        {
            public Gender Gender { get; }

            public GenderWrapper(Gender gender)
            {
                Gender = gender;
            }

            public override string ToString()
            {
                return Gender.Description();
            }
        }

        ~SettingViewModel()
        {
            Dispose(false);
        }
    }
}