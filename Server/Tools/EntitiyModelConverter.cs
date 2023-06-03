using Microsoft.EntityFrameworkCore;
using Server.Entities;
using Server.Models;

namespace Server.Tools;

public static class EntitiyModelConverter
{
    public static User ConvertToModel(this UserEntity userEntity)
    {
        return new User()
        {
            Id = userEntity.Id,
            Username = userEntity.Username,
            Firstname = userEntity.Firstname,
            Lastname = userEntity.Lastname,
            Gender = userEntity.Gender,
            Birthdate = userEntity.Birthdate,
            Image = userEntity.Image?.ConvertToModel(),
            Status = userEntity.Status,
            IsOnline = userEntity.IsOnline,
            LastActivity = userEntity.LastActivity
        };
    }

    public static Message ConvertToModel(this MessageEntity messageEntity)
    {
        return new()
        {
            Id = messageEntity.Id,
            SenderUsername = messageEntity.Sender.Username,
            ReceiverUsername = messageEntity.Receiver.Username,
            Content = messageEntity.Content.ConvertToModel(),
            Created = messageEntity.Created,
            IsEdited = messageEntity.IsEdited,
            IsReceived = messageEntity.IsReceived,
            IsRead = messageEntity.IsRead,
            IsDeleted = messageEntity.IsDeleted
        };
    }

    public static MessageContent ConvertToModel(this MessageContentEntity messageContentEntity)
    {
        List<ImageModel>? images = null;

        if (messageContentEntity.Images is not null)
        {
            images = new();

            foreach (var imageEntity in messageContentEntity.Images)
            {
                images.Add(imageEntity.ConvertToModel());
            }
        }

        return new()
        {
            Id = messageContentEntity.Id,
            Text = messageContentEntity.Text,
            Images = images
        };
    }

    public static ImageModel ConvertToModel(this ImageEntity imageEntity)
    {
        return new()
        {
            Id = imageEntity.Id,
            Name = imageEntity.Name,
            Path = imageEntity.Path,
        };
    }

    public static UserEntity? TryGetEntity(this User user)
    {
        using ApplicationContext appContext = new();
        return appContext.Users.Include(u => u.Image).SingleOrDefault(u => u.Id == user.Id);
    }

    public static MessageEntity? TryGetEntity(this Message message)
    {
        using ApplicationContext appContext = new();
        return appContext.Messages.Include(m => m.Sender.Image).Include(m => m.Receiver.Image).Include(m => m.Content.Images).SingleOrDefault(m => m.Id == message.Id);
    }

    public static MessageContentEntity? TryGetEntity(this MessageContent messageContent)
    {
        using ApplicationContext appContext = new();
        return appContext.MessagesContents.Include(mC => mC.Images).SingleOrDefault(mC => mC.Id == messageContent.Id);
    }

    public static ImageEntity? TryGetEntity(this ImageModel image)
    {
        using ApplicationContext appContext = new();
        return appContext.Images.SingleOrDefault(i => i.Id == image.Id);
    }
}