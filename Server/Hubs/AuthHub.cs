using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Args;
using Server.Models;
using Server.Tools;

namespace Server.Hubs;

public class AuthHub : Hub
{
    [AllowAnonymous]
    public async Task Auth(AuthArgs args)
    {
        if (Database.CheckAuthArgsValid(args, out User? user, out string? errorMessage) is false)
        {
            await Clients.Caller.SendAsync("ReceiveErrorMessage", errorMessage);
            return;
        }

        user!.IsOnline = true;
        string token = TokenGenerator.GetNewToken(user);
        await Clients.Caller.SendAsync("ReceiveToken", token, user);
    }

    [AllowAnonymous]
    public async Task Register(RegisterArgs args)
    {
        if (Database.RegisterUser(args, out User? user, out string? errorMessage) is false)
        {
            await Clients.Caller.SendAsync("ReceiveErrorMessage", errorMessage);
            return;
        }

        user!.IsOnline = true;
        string token = TokenGenerator.GetNewToken(user);
        await Clients.Caller.SendAsync("ReceiveToken", token, user);
    }
}
