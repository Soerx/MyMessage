using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Server.Tools;

public class ClaimUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
    }
}
