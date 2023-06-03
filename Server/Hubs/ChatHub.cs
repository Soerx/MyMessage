using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Models;

namespace Server.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public async Task SyncData()
    {
        if (Database.GetUserMessages(Context.UserIdentifier!, out List<Message>? messages, out string? errorMessage) is false)
        {
            await Clients.Caller.SendAsync("ReceiveErrorMessage", errorMessage);
            return;
        }

        var users = Database.GetAllUsersList();
        var currentUser = users.First(u => u.Username == Context.UserIdentifier);
        users.Remove(currentUser);

        Data data = new()
        {
            Messages = messages!,
            Users = users,
        };

        await Clients.Caller.SendAsync("SyncData", data);
    }

    public async Task SendMessage(MessageContent messageContent, string receiverUsername)
    {
        if (Database.AddMessage(Context.UserIdentifier!, receiverUsername, messageContent, out Message? insertedMessage, out string? errorMessage) is false)
        {
            await Clients.Caller.SendAsync("ReceiveErrorMessage", errorMessage);
            return;
        }

        await Clients.User(Context.UserIdentifier!).SendAsync("ReceiveMessageData", insertedMessage);
        await Clients.User(receiverUsername).SendAsync("ReceiveMessageData", insertedMessage);
    }

    public async Task UpdateMessage(Message updatedMessage)
    {
        if (Database.UpdateMessage(Context.UserIdentifier!, ref updatedMessage, out string? errorMessage) is false)
        {
            await Clients.Caller.SendAsync("ReceiveErrorMessage", errorMessage);
            return;
        }

        await Clients.User(updatedMessage.SenderUsername).SendAsync("ReceiveMessageData", updatedMessage);
        await Clients.User(updatedMessage.ReceiverUsername).SendAsync("ReceiveMessageData", updatedMessage);
    }

    public async Task UpdateCurrentUser(User updatedUser)
    {
        if (Database.UpdateUser(Context.UserIdentifier!, ref updatedUser, out string? errorMessage) is false)
        {
            await Clients.Caller.SendAsync("ReceiveErrorMessage", errorMessage);
            return;
        }

        updatedUser.IsOnline = true;
        await Clients.All.SendAsync("ReceiveUserData", updatedUser);
    }

    public async Task UpdateCurrentUserPassword(string currentPassword, string newPassword)
    {
        if (Database.UpdateUserPassword(Context.UserIdentifier!, currentPassword, newPassword, out string? errorMessage) is false)
        {
            await Clients.Caller.SendAsync("ReceiveErrorMessage", errorMessage);
            return;
        }

        await Clients.All.SendAsync("ReceiveConfirmation");
    }

    public override Task OnConnectedAsync()
    {
        Database.SetUserConnectionStatus(Context.UserIdentifier!, isOnline: true);
        Database.GetUser(Context.UserIdentifier!, out User? user, out _);
        Clients.All.SendAsync("ReceiveUserData", user);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Database.SetUserConnectionStatus(Context.UserIdentifier!, isOnline: false);
        Database.GetUser(Context.UserIdentifier!, out User? user, out _);
        Clients.All.SendAsync("ReceiveUserData", user);
        return base.OnDisconnectedAsync(exception);
    }
}
