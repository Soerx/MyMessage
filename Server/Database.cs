
using Microsoft.EntityFrameworkCore;
using Server.Models;
using Server.Entities;
using Server.Tools;
using Server.Args;

namespace Server;

public static class Database
{
    private const int MIN_USERNAME_LENGTH = 2;
    private const int MAX_USERNAME_LENGTH = 20;
    private const int MIN_PASSWORD_LENGTH = 8;
    private const int MAX_PASSWORD_LENGTH = 52;
    private const int MIN_FIRSTNAME_LENGTH = 2;
    private const int MAX_FIRSTNAME_LENGTH = 20;
    private const int MIN_LASTNAME_LENGTH = 2;
    private const int MAX_LASTNAME_LENGTH = 20;
    private const string INVALID_ARGUMENTS_ERROR_MESSAGE = "Один или несколько аргументов отсутствуют или имеют неверный формат.";
    private const string NOT_UNIQUE_USERNAME_ERROR_MESSAGE = "Пользователь с таким логином уже существует.";
    private const string INVALID_USERNAME_OR_PASSWORD_ERROR_MESSAGE = "Неверный логин или пароль.";
    private const string INVALID_CURRENT_PASSWORD_ERROR_MESSAGE = "Текущий пароль указан неверно.";
    private const string USER_NOT_FOUND_ERROR_MESSAGE = "Пользователь не найден.";
    private const string NO_ACCESS_RIGHTS_ERROR_MESSAGE = "Пользователь не обладает правами доступа для выполнения этого действия.";
    private const string SALT = "t~vx#KMmlzwOi#*paCWA";

    public static bool RegisterUser(RegisterArgs args, out User? user, out string? errorMessage)
    {
        errorMessage = null;
        user = null;

        if (CheckDataCorrect(args) is false)
        {
            errorMessage = INVALID_ARGUMENTS_ERROR_MESSAGE;
            return false;
        }

        if (GetUser(args.Username, out _, out _))
        {
            errorMessage = NOT_UNIQUE_USERNAME_ERROR_MESSAGE;
            return false;
        }

        var userEntity = new UserEntity()
        {
            Username = args.Username,
            Password = SHA256Calculator.Calculate(args.Password + SALT + args.Username),
            Firstname = args.Firstname,
            Lastname = args.Lastname,
            Gender = args.Gender,
            Birthdate = args.Birthdate,
            LastActivity = DateTime.Now
        };

        using ApplicationContext appContext = new();
        appContext.Users.Add(userEntity);
        appContext.SaveChanges();
        user = userEntity.ConvertToModel();
        return true;
    }

    public static bool CheckAuthArgsValid(AuthArgs args, out User? user, out string? errorMessage)
    {
        errorMessage = null;
        user = null;

        if (CheckDataCorrect(args) is false)
        {
            errorMessage = INVALID_ARGUMENTS_ERROR_MESSAGE;
            return false;
        }

        using ApplicationContext appContext = new();
        appContext.Users.Load();
        UserEntity? userEntity = appContext.Users.Include(u => u.Image).SingleOrDefault(u => u.Username == args.Username
            && u.Password.SequenceEqual(SHA256Calculator.Calculate(args.Password + SALT + args.Username)));

        if (userEntity is null)
        {
            errorMessage = INVALID_USERNAME_OR_PASSWORD_ERROR_MESSAGE;
            return false;
        }

        user = userEntity.ConvertToModel();
        return true;
    }

    public static bool AddMessage(string senderUsername, string receiverUsername, MessageContent insertingContent, out Message? insertedMessage, out string? errorMessage)
    {
        errorMessage = null;
        insertedMessage = null;
        using ApplicationContext appContext = new();
        appContext.Users.Load();
        UserEntity? sender = appContext.Users.SingleOrDefault(u => u.Username == senderUsername);
        UserEntity? receiver = appContext.Users.SingleOrDefault(u => u.Username == receiverUsername);

        if (sender is null
            || receiver is null
            || insertingContent is null
            || (string.IsNullOrWhiteSpace(insertingContent.Text)
            && (insertingContent.Images is null
            || insertingContent.Images.Count == 0)))
        {
            errorMessage = INVALID_ARGUMENTS_ERROR_MESSAGE;
            return false;
        }

        List<ImageEntity>? imagesEntities = null;

        if (insertingContent.Images is not null)
        {
            imagesEntities = new List<ImageEntity>();

            foreach (var image in insertingContent.Images)
            {
                var imageEntity = image.TryGetEntity();

                if (imageEntity is not null)
                {
                    appContext.Attach(imageEntity);
                    imagesEntities.Add(imageEntity);
                }
            }
        }

        MessageContentEntity messageContentEntity = new()
        {
            Text = insertingContent.Text,
            Images = imagesEntities,
        };

        MessageEntity messageEntity = new()
        {
            Sender = sender,
            Receiver = receiver,
            Content = messageContentEntity,
            Created = DateTime.Now
        };

        appContext.Messages.Add(messageEntity);
        appContext.SaveChanges();
        messageEntity = appContext.Messages.Include(m => m.Receiver).Include(m => m.Sender).Include(m => m.Content.Images).First(m => m.Id == messageEntity.Id);
        insertedMessage = messageEntity.ConvertToModel();
        return true;
    }

    public static bool GetUser(string username, out User? user, out string? errorMessage)
    {
        errorMessage = null;
        user = null;

        if (string.IsNullOrWhiteSpace(username))
        {
            errorMessage = INVALID_ARGUMENTS_ERROR_MESSAGE;
            return false;
        }

        using ApplicationContext db = new();
        db.Users.Load();
        UserEntity? userEntity = db.Users.Include(u => u.Image).SingleOrDefault(u => u.Username == username);

        if (userEntity is null)
        {
            errorMessage = USER_NOT_FOUND_ERROR_MESSAGE;
            return false;
        }

        user = userEntity.ConvertToModel();
        return true;
    }

    public static bool UpdateUser(string username, ref User updatedUser, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(username)
            || CheckDataCorrect(updatedUser) is false)
        {
            errorMessage = INVALID_ARGUMENTS_ERROR_MESSAGE;
            return false;
        }

        ApplicationContext db = new();
        db.Users.Load();
        var tempUser = updatedUser;
        UserEntity? realUser = db.Users.Include(u => u.Image).SingleOrDefault(u => u.Username == username && u.Username == tempUser.Username);

        if (realUser is null)
        {
            errorMessage = USER_NOT_FOUND_ERROR_MESSAGE;
            return false;
        }

        var imageEmtity = updatedUser.Image?.TryGetEntity();

        if (imageEmtity is not null && realUser.Image is not null && imageEmtity.Id != realUser.Image.Id)
            realUser.Image = imageEmtity;

        realUser.Firstname = updatedUser.Firstname;
        realUser.Lastname = updatedUser.Lastname;
        realUser.Birthdate = updatedUser.Birthdate;
        realUser.Gender = updatedUser.Gender;
        realUser.Status = updatedUser.Status;
        updatedUser = db.Users.Include(u => u.Image).First(u => u.Username == username && u.Username == tempUser.Username).ConvertToModel();
        db.SaveChanges();
        return true;
    }

    public static bool UpdateUserPassword(string username, string currentPassword, string newPpassword, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(username)
            || CheckTextFieldCorrect(newPpassword, MIN_PASSWORD_LENGTH, MAX_PASSWORD_LENGTH) is false)
        {
            errorMessage = INVALID_ARGUMENTS_ERROR_MESSAGE;
            return false;
        }

        ApplicationContext db = new();
        db.Users.Load();
        UserEntity? realUser = db.Users.SingleOrDefault(u => u.Username == username);

        if (realUser is null)
        {
            errorMessage = USER_NOT_FOUND_ERROR_MESSAGE;
            return false;
        }

        if (realUser.Password.SequenceEqual(SHA256Calculator.Calculate(currentPassword + SALT + username)) is false)
        {
            errorMessage = INVALID_CURRENT_PASSWORD_ERROR_MESSAGE;
            return false;
        }

        realUser.Password = SHA256Calculator.Calculate(newPpassword + SALT + username);
        db.SaveChanges();
        return true;
    }

    public static bool SetUserConnectionStatus(string username, bool isOnline)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        ApplicationContext db = new();
        db.Users.Load();
        UserEntity? user = db.Users.SingleOrDefault(u => u.Username == username);

        if (user is null)
            return false;

        user.IsOnline = isOnline;
        user.LastActivity = DateTime.Now;
        db.SaveChanges();
        return true;
    }

    public static bool UpdateMessage(string username, ref Message updatedMessage, out string? errorMessage)
    {
        errorMessage = null;

        if (GetUser(username, out User? user, out errorMessage) is false
            || updatedMessage is null
            || updatedMessage.Content is null
            || (updatedMessage.Content.Text is null
            && (updatedMessage.Content.Images is null
            || updatedMessage.Content.Images.Count == 0)))
        {
            errorMessage = INVALID_ARGUMENTS_ERROR_MESSAGE;
            return false;
        }

        var tempUpdatedMessage = updatedMessage;
        ApplicationContext appContext = new();
        appContext.Messages.Load();
        MessageEntity? realMessage = appContext.Messages.Include(m => m.Receiver).Include(m => m.Sender).Include(m => m.Content.Images).SingleOrDefault(m => m.Id == tempUpdatedMessage.Id);

        if (realMessage is null
            || (realMessage.Sender.Username != username
            && realMessage.Receiver.Username != username))
        {
            errorMessage = NO_ACCESS_RIGHTS_ERROR_MESSAGE;
            return false;
        }

        if (realMessage.Sender.Username == username)
        {
            realMessage.Content.Text = updatedMessage.Content.Text;

            if (updatedMessage.Content.Images is not null)
            {
                realMessage.Content.Images ??= new();

                foreach (var imageModel in updatedMessage.Content.Images)
                {
                    ImageEntity? imageEntity = imageModel.TryGetEntity();

                    if (imageEntity is not null)
                    {
                        if (realMessage.Content.Images.All(i => i.Id != imageEntity.Id))
                        {
                            appContext.Attach(imageEntity);
                            realMessage.Content.Images.Add(imageEntity);
                        }
                    }
                }

                List<ImageEntity> removingImages = new();

                foreach (var imageEntity in realMessage.Content.Images)
                {
                    if (updatedMessage.Content.Images.All(i => i.Id != imageEntity.Id))
                        removingImages.Add(imageEntity);
                }

                int removingImagesCount = removingImages.Count;

                for (int i = 0; i < removingImagesCount; i++)
                {
                    realMessage.Content.Images.Remove(removingImages[0]);
                }
            }

            realMessage.IsDeleted = realMessage.IsDeleted || updatedMessage.IsDeleted;
            realMessage.IsEdited = realMessage.IsEdited || updatedMessage.IsEdited;
        }
        else
        {
            realMessage.IsRead = realMessage.IsRead || updatedMessage.IsRead;
            realMessage.IsReceived = true;
        }

        updatedMessage = appContext.Messages.Include(m => m.Receiver).Include(m => m.Sender).Include(m => m.Content.Images).First(m => m.Id == realMessage.Id).ConvertToModel();
        appContext.SaveChanges();
        return true;
    }

    public static List<User> GetAllUsersList()
    {
        List<User> users = new List<User>();
        using ApplicationContext db = new();
        db.Users.Load();

        foreach (UserEntity usrEnt in db.Users.Include(u => u.Image))
        {
            users.Add(usrEnt.ConvertToModel());
        }

        return users;
    }

    public static bool GetUserMessages(string username, out List<Message>? messages, out string? errorMessage)
    {
        messages = null;

        if (GetUser(username, out _, out errorMessage) is false)
            return false;

        messages = new List<Message>();
        using ApplicationContext db = new();
        db.Messages.Load();

        foreach (MessageEntity msgEnt in db.Messages.Include(m => m.Receiver).Include(m => m.Sender).Include(m => m.Content.Images))
        {
            if (msgEnt.Sender.Username == username || msgEnt.Receiver.Username == username)
            {
                messages.Add(msgEnt.ConvertToModel());

                if (msgEnt.IsDeleted)
                {
                    msgEnt.Content.Text = null;
                    msgEnt.Content.Images = null;
                }
            }
        }

        return true;
    }

    private static bool CheckDataCorrect(AuthArgs args)
    {
        if (args is null
            || CheckTextFieldCorrect(args.Username, MIN_USERNAME_LENGTH, MAX_USERNAME_LENGTH) is false
            || CheckTextFieldCorrect(args.Password, MIN_PASSWORD_LENGTH, MAX_PASSWORD_LENGTH) is false)
            return false;

        return true;
    }

    private static bool CheckDataCorrect(User user)
    {
        if (user is null
            || CheckTextFieldCorrect(user.Username, MIN_USERNAME_LENGTH, MAX_USERNAME_LENGTH) is false
            || CheckTextFieldCorrect(user.Firstname, MIN_FIRSTNAME_LENGTH, MAX_FIRSTNAME_LENGTH) is false
            || CheckTextFieldCorrect(user.Lastname, MIN_LASTNAME_LENGTH, MAX_LASTNAME_LENGTH) is false)
            return false;

        string minBirthdateDate = "01.01.1900";
        int minUserAge = 14;

        if (user.Birthdate.Date < DateTime.Parse(minBirthdateDate) &&
            user.Birthdate.Date > DateTime.Now.AddYears(-minUserAge))
            return false;

        return true;
    }

    private static bool CheckDataCorrect(RegisterArgs args)
    {
        if (args is null
            || CheckTextFieldCorrect(args.Username, MIN_USERNAME_LENGTH, MAX_USERNAME_LENGTH) is false
            || CheckTextFieldCorrect(args.Password, MIN_PASSWORD_LENGTH, MAX_PASSWORD_LENGTH) is false
            || CheckTextFieldCorrect(args.Firstname, MIN_FIRSTNAME_LENGTH, MAX_FIRSTNAME_LENGTH) is false
            || CheckTextFieldCorrect(args.Lastname, MIN_LASTNAME_LENGTH, MAX_LASTNAME_LENGTH) is false)
            return false;

        string minBirthdateDate = "01.01.1900";
        int minUserAge = 14;

        if (args.Birthdate.Date < DateTime.Parse(minBirthdateDate) &&
            args.Birthdate.Date > DateTime.Now.AddYears(-minUserAge))
            return false;

        return true;
    }

    private static bool CheckTextFieldCorrect(string fieldValue, int minValueLength, int maxValueLength)
    {
        if (string.IsNullOrWhiteSpace(fieldValue)
            || fieldValue.Length < minValueLength
            || fieldValue.Length > maxValueLength)
            return false;

        return true;
    }
}