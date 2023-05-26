using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Server.Models;

namespace Server.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public async Task SyncData()
    {
        if (Context.UserIdentifier is null)
            return;

        using ApplicationContext db = new();
        db.Users.Load();
        User? currentUser = db.Users.SingleOrDefault(u => u.Username == Context.UserIdentifier);

        if (currentUser is null)
            return;

        db.Ranks.Load();
        db.Chats.Load();
        db.Messages.Load();
        List<User> userListWithoutCurrentUser = db.Users.ToList();
        userListWithoutCurrentUser.Remove(currentUser);

        List<Chat> chatListAvailableCurrentUser = db.Chats.Where(c => c.Users.Any(u => u.Id == currentUser.Id)).Include(c => c.Users).Include(c => c.Messages).ToList();

        foreach (Chat cht in chatListAvailableCurrentUser)
        {
            foreach (User usr in cht.Users)
            {
                usr.Chats = null;
            }
        }

        foreach (Chat cht in chatListAvailableCurrentUser)
        {
            if (cht.Messages is not null)
            {
                foreach (Message msg in cht.Messages)
                {
                    msg.Chat = null;
                    msg.Sender.Chats = null;
                    msg.Content = db.Messages.Include(m => m.Content).First(m => m.Id == msg.Id).Content;
                }
            }
        }

        Data data = new()
        {
            Users = userListWithoutCurrentUser,
            Chats = chatListAvailableCurrentUser,
        };

        await Clients.Caller.SendAsync("SyncData", data);
    }

    public async Task SendMessage(string messageText, User? receiver = null, Chat? destinationChat = null)
    {
        if (Context.UserIdentifier is null || messageText is null)
            return;

        if (receiver is null && destinationChat is null)
            return;

        using ApplicationContext db = new();
        db.Users.Load();
        var dbUsers = db.Users.Include(u => u.Chats).Include(u => u.Rank);

        User? sender = dbUsers.SingleOrDefault(u => u.Username == Context.UserIdentifier);
        User? realReceiver = null;

        if (receiver is not null)
            realReceiver = dbUsers.SingleOrDefault(u => u.Id == receiver.Id);

        if (sender is null)
            return;

        db.Chats.Load();
        var dbChats = db.Chats.Include(c => c.Users).Include(c => c.Messages);

        if (destinationChat is not null)
        {
            Chat? realChat = dbChats.SingleOrDefault(c => c.Id == destinationChat.Id);

            if (realChat is null || realChat.Users.Any(u => u.Id == sender.Id) is false)
                return;

            destinationChat = realChat;
        }
        else if (realReceiver is not null)
        {
            int dialogUsersCount = 2;
            var foundChatsList = dbChats.Where(c => c.Users.Any(u => u.Id == sender.Id) && c.Users.Count == dialogUsersCount);
            List<Chat> senderChatsList = dbChats.Where(c => c.Users.Any(u => u.Id == sender.Id)).ToList();
            List<Chat> senderDialogsList = senderChatsList.Where(c => c.Users.Count == dialogUsersCount).ToList();
            destinationChat = senderDialogsList.SingleOrDefault(c => c.Users.Any(u => u.Id == realReceiver.Id));

            if (destinationChat is null)
            {
                destinationChat = new Chat
                {
                    Users = new List<User>()
                    {
                        sender,
                        realReceiver
                    }
                };

                db.Chats.Add(destinationChat);
                db.SaveChanges();

                var sendingUsersData = new List<User>(destinationChat.Users);
                var temp = new List<User>();

                foreach (var user in sendingUsersData)
                {
                    temp.Add(user);
                    user.Chats = null;
                }

                var sendingChatData = new Chat()
                {
                    Users = sendingUsersData
                };

                foreach (var u in destinationChat.Users)
                {
                    await Clients.User(u.Username).SendAsync("ReceiveChatData", sendingChatData);
                }

                sender.Chats ??= new List<Chat>();
            }
        }

        if (destinationChat is not null)
        {
            var msgContent = new MessageContent()
            {
                Text = messageText
            };

            var msg = new Message()
            {
                Chat = destinationChat,
                Content = msgContent,
                Created = DateTime.Now,
                Sender = sender,
                IsReceived = false,
                IsRead = false,
                IsEdited = false,
                IsDeleted = false
            };

            destinationChat.Messages ??= new List<Message>();
            destinationChat.Messages.Add(msg);
            db.SaveChanges();


            var sendingUserData = new User()
            {
                Id = sender.Id,
                Username = sender.Username,
                Birthdate = sender.Birthdate,
                Firstname = sender.Firstname,
                Lastname = sender.Lastname,
                Gender = sender.Gender,
                Image = sender.Image,
                LastActivity = sender.LastActivity,
                Rank = sender.Rank,
                IsOnline = sender.IsOnline,
                Status = sender.Status,
            };

            var sendingMessageData = new Message()
            {
                Content = msg.Content,
                Created = msg.Created,
                Sender = sendingUserData,
                IsReceived = msg.IsReceived,
                IsRead = msg.IsRead,
                IsEdited = msg.IsEdited,
                IsDeleted = msg.IsDeleted
            };

            foreach (User usr in destinationChat.Users)
            {
                await Clients.User(usr.Username).SendAsync("ReceiveMessageData", sendingMessageData);
            }
        }
    }

    public async Task UpdateMessage(Message updatedMessage)
    {
        if (Context.UserIdentifier is null || updatedMessage is null)
            return;

        using ApplicationContext db = new();
        db.Users.Load();
        User? currentUser = db.Users.SingleOrDefault(u => u.Username == Context.UserIdentifier);

        if (currentUser is null)
            return;

        db.Chats.Load();
        Chat? destinationChat = db.Chats.SingleOrDefault(c => c.Id == updatedMessage.Chat.Id);

        if (destinationChat is null || destinationChat.Messages is null)
            return;

        Message? realMessage = destinationChat.Messages.SingleOrDefault(m => m.Id == updatedMessage.Id);

        if (realMessage is null)
            return;

        if (realMessage.Sender.Id == currentUser.Id)
        {
            realMessage.IsEdited = true;
            realMessage.IsDeleted = realMessage.IsDeleted ? realMessage.IsDeleted : updatedMessage.IsDeleted;
            realMessage.Content = updatedMessage.Content;
            db.SaveChanges();

            foreach (User usr in realMessage.Chat.Users)
            {
                await Clients.User(usr.Username).SendAsync("ReceiveMessageData", realMessage);
            }
        }
        else if (destinationChat.Users.Any(u => u.Id == currentUser.Id))
        {
            realMessage.IsRead = realMessage.IsRead ? realMessage.IsRead : realMessage.IsRead;
            realMessage.IsReceived = true;
            db.SaveChanges();

            foreach (User usr in realMessage.Chat.Users)
            {
                await Clients.User(usr.Username).SendAsync("ReceiveMessageData", realMessage);

            }
        }
    }

    public async Task UpdateChat(Chat updatedChat)
    {
        if (Context.UserIdentifier is null || updatedChat is null)
            return;

        using ApplicationContext db = new();
        db.Users.Load();
        User? currentUser = db.Users.SingleOrDefault(u => u.Username == Context.UserIdentifier);

        if (currentUser is null)
            return;

        db.Chats.Load();
        Chat? realChat = db.Chats.SingleOrDefault(c => c.Id == updatedChat.Id);

        if (realChat is null)
            return;

        if (realChat.Users.Any(u => u.Username == currentUser.Username))
        {
            realChat.Name = updatedChat.Name;
            db.SaveChanges();

            foreach (User usr in realChat.Users)
            {
                await Clients.User(usr.Username).SendAsync("ReceiveChatData", realChat);
            }
        }
    }

    public async Task UpdateCurrentUser(User updatedUser)
    {
        if (Context.UserIdentifier is null || updatedUser is null)
            return;

        using ApplicationContext db = new();
        db.Users.Load();
        User? currentUser = db.Users.SingleOrDefault(u => u.Username == Context.UserIdentifier);

        if (currentUser is null || currentUser.Id != updatedUser.Id)
            return;

        currentUser.Firstname = updatedUser.Firstname;
        currentUser.Lastname = updatedUser.Lastname;
        currentUser.Gender = updatedUser.Gender;
        currentUser.Birthdate = updatedUser.Birthdate;
        currentUser.Status = updatedUser.Status;
        currentUser.Image = updatedUser.Image;
        db.SaveChanges();
        await Clients.All.SendAsync("ReceiveUserData", currentUser);
    }

    public override Task OnConnectedAsync()
    {
        if (Context.UserIdentifier is null)
            return base.OnConnectedAsync();

        using ApplicationContext db = new();
        db.Ranks.Load();
        db.Users.Load();
        User user = db.Users.First(u => u.Username == Context.UserIdentifier);
        user.IsOnline = true;
        db.SaveChanges();
        Clients.All.SendAsync("ReceiveUserData", user);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.UserIdentifier is null)
            return base.OnDisconnectedAsync(exception);

        using ApplicationContext db = new();
        db.Ranks.Load();
        db.Users.Load();
        User user = db.Users.First(u => u.Username == Context.UserIdentifier);
        user.LastActivity = DateTime.Now;
        user.IsOnline = false;
        db.SaveChanges();
        Clients.All.SendAsync("ReceiveUserData", user);
        return base.OnDisconnectedAsync(exception);
    }
}
