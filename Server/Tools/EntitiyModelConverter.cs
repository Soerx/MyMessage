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
            Image = userEntity.Image,
            Status = userEntity.Status,
            IsOnline = userEntity.IsOnline,
            LastActivity = userEntity.LastActivity
        };
    }

    public static Message ConvertToModel(this MessageEntity messageEntity)
    {
        return new Message()
        {
            Id = messageEntity.Id,
            SenderUsername = messageEntity.Sender.Username,
            ReceiverUsername = messageEntity.Receiver.Username,
            ContentId = messageEntity.Content.Id,
            Created = messageEntity.Created,
            IsEdited = messageEntity.IsEdited,
            IsReceived = messageEntity.IsReceived,
            IsRead = messageEntity.IsRead,
            IsDeleted = messageEntity.IsDeleted
        };
    }

    public static MessageContent ConvertToModel(this MessageContentEntity messageContentEntity)
    {
        List<byte[]>? imagesDataArrays = null;

        if (messageContentEntity.Images is not null)
        {
            imagesDataArrays = new List<byte[]>();

            foreach (var image in messageContentEntity.Images)
            {
                imagesDataArrays.Add(image.Data);
            }
        }

        return new MessageContent()
        {
            Id = messageContentEntity.Id,
            Text = messageContentEntity.Text,
            Images = imagesDataArrays
        };
    }
}