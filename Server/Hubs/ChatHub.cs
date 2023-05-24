using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
        db.Ranks.Load();
        db.Users.Load();
        db.Chats.Load();
        db.MessagesContents.Load();
        db.Messages.Load();
        var currentUser = db.Users.First(u => u.Username == Context.UserIdentifier);
        var userListWithoutCurrentUser = db.Users.ToList();
        userListWithoutCurrentUser.Remove(currentUser);

        foreach (var tmpUsr in  userListWithoutCurrentUser)
        {
            tmpUsr.Chats = null;
        }

        var chatListAvailableCurrentUser = db.Chats.Where(c => c.Users.Any(u => u.Id == currentUser.Id)).ToList();

        Data data = new()
        {
            Users = userListWithoutCurrentUser,
            Messages = db.Messages,
            Chats = chatListAvailableCurrentUser,
        };

        await Clients.Caller.SendAsync("SyncData", data);
    }

    public async Task SendMessage(string messageText, User? receiver = null, Chat? chat = null)
    {
        if (Context.UserIdentifier is null)
            return;

        if (receiver is null && chat is null)
            return;

        using ApplicationContext db = new();
        db.Users.Load();
        var user = db.Users.First(u => u.Username == Context.UserIdentifier);

        if (chat is null && receiver is not null)
        {
            db.Chats.Load();
            var foundChats = db.Chats.Where(c => c.Users.Any(u => u.Id == receiver.Id) && c.Users.Count == 2);

            if (foundChats.Any(c => c.Users.Any(u => u.Id == user.Id)))
            {
                chat = foundChats.First(c => c.Users.Any(u => u.Id == user.Id));
            }
            else
            {
                chat = new Chat()
                {
                    Users = new List<User>()
                };

                chat.Users.Add(receiver);
                chat.Users.Add(user);
                db.Chats.Add(chat);

                foreach (var u in chat.Users)
                {
                    await Clients.User(u.Username).SendAsync("ReceiveChatData", chat);
                }
            }
        }

        if (chat is not null)
        {
            var msgContent = new MessageContent()
            {
                Text = messageText
            };

            db.MessagesContents.Add(msgContent);

            var msg = new Message()
            {
                Chat = chat,
                Content = msgContent,
                Created = DateTime.Now,
                Sender = user,
                IsReceived = false,
                IsRead = false,
                IsEdited = false,
                IsDeleted = false
            };

            db.Messages.Add(msg);
            db.SaveChanges();

            foreach (var u in chat.Users)
            {
                await Clients.User(u.Username).SendAsync("ReceiveMessageData", msg);
            }
        }
    }

    public async Task UpdateMessage(Message updatedMessage)
    {
        if (Context.UserIdentifier is null)
            return;

        using ApplicationContext db = new();
        db.Chats.Load();
        db.Users.Load();
        db.Messages.Load();
        var sender = db.Users.First(u => u.Username == Context.UserIdentifier);

        if (db.Messages.Any(m => m.Id == updatedMessage.Id))
        {
            if (db.Messages.Any(m => m.Sender.Id == sender.Id))
            {
                var msg = db.Messages.First(m => m.Id == updatedMessage.Id);
                msg.IsEdited = true;
                msg.IsDeleted = msg.IsDeleted ? true : updatedMessage.IsDeleted;
                msg.Content = updatedMessage.Content;
                db.SaveChanges();

                foreach (var user in msg.Chat.Users)
                {
                    await Clients.User(user.Username).SendAsync("ReceiveMessageData", msg);
                }
            }
            else if (db.Messages.Any(m => m.Chat.Users.Any(u => u.Id == sender.Id)))
            {
                var msg = db.Messages.First(m => m.Id == updatedMessage.Id);
                msg.IsRead = msg.IsRead ? true : msg.IsRead;
                msg.IsReceived = true;
                db.SaveChanges();

                foreach (var user in msg.Chat.Users)
                {
                    await Clients.User(user.Username).SendAsync("ReceiveMessageData", msg);

                }
            }
        }
    }

    public async Task UpdateChat(Chat updatedChat)
    {
        if (Context.UserIdentifier is null)
            return;

        using ApplicationContext db = new();
        db.Chats.Load();
        db.Users.Load();

        if (db.Chats.Any(c => c.Id == updatedChat.Id))
        {
            var foundChat = db.Chats.First(c => c.Id == updatedChat.Id);
            var user = db.Users.First(u => u.Username == Context.UserIdentifier);
            
            if (foundChat.Users.Any(u => u.Username == user.Username))
            {
                foundChat.Name = updatedChat.Name;
                db.SaveChanges();

                foreach (var u in foundChat.Users)
                {
                    await Clients.User(u.Username).SendAsync("ReceiveChatData", foundChat);
                }
            }
        }
    }

    public async Task UpdateCurrentUser(User updatedUser)
    {
        if (Context.UserIdentifier is null)
            return;

        using ApplicationContext db = new();
        db.Chats.Load();
        db.Users.Load();
        var user = db.Users.First(u => u.Username == Context.UserIdentifier);
        user.Firstname = updatedUser.Firstname;
        user.Lastname = updatedUser.Lastname;
        user.Gender = updatedUser.Gender;
        user.Birthdate = updatedUser.Birthdate;
        user.Status = updatedUser.Status;
        user.Image = updatedUser.Image;
        db.SaveChanges();
        user.Chats = null;
        await Clients.All.SendAsync("ReceiveUserData", user);
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
