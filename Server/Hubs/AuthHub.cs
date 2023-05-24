using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Args;
using Server.Models;
using Server.Tools;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Server.Hubs;

public class AuthHub : Hub
{
    [AllowAnonymous]
    public async Task Auth(AuthArgs args)
    {
        if (args is null)
        {
            await Clients.Caller.SendAsync("ReceiveBadAuthResponse", "Аргументы не могут быть пустыми.");
            return;
        }

        using ApplicationContext db = new();
        db.Ranks.Load();
        db.Users.Load();
        User? user = db.Users.SingleOrDefault(u => u.Username == args.Username);

        if (user is null)
        {
            await Clients.Caller.SendAsync("ReceiveBadAuthResponse", "Пользователя с таким логином не существует.");
            return;
        }

        db.Passwords.Load();
        UserPassword rightPass = db.Passwords.First(p => p.User.Id == user.Id);
        byte[] argsPass = SHA256Calculator.Calculate(args.Password);

        if (rightPass is null || rightPass.Password.SequenceEqual(argsPass) == false)
        {
            await Clients.Caller.SendAsync("ReceiveBadAuthResponse", "Неверный логин или пароль.");
            return;
        }

        user.IsOnline = true;
        List<Claim> claims = new() { new Claim(ClaimTypes.Name, args.Username) };
        JwtSecurityToken jwt = new(
                issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        await Clients.Caller.SendAsync("ReceiveToken", encodedJwt, user);
    }

    [AllowAnonymous]
    public async Task Register(RegisterArgs args)
    {
        if (args is null)
        {
            await Clients.Caller.SendAsync("ReceiveBadAuthResponse", "Аргументы не могут быть пустыми.");
            return;
        }

        using ApplicationContext db = new();
        db.Users.Load();
        db.Ranks.Load();

        if (db.Users.Any(u => u.Username == args.Username))
        {
            await Clients.Caller.SendAsync("ReceiveBadAuthResponse", "Пользователь с таким логином уже существует.");
            return;
        }

        int userRankId = 1;

        var user = new User()
        { 
            Rank = db.Ranks.First(r => r.Id == userRankId),
            Username = args.Username,
            Firstname = args.Firstname,
            Lastname = args.Lastname,
            Gender = args.Gender,
            Birthdate = args.Birthdate,
            LastActivity = DateTime.Now
        };

        var userPass = new UserPassword()
        {
            User = user,
            Password = SHA256Calculator.Calculate(args.Password)
        };

        db.Users.Add(user);
        db.Passwords.Add(userPass);
        db.SaveChanges();

        user.IsOnline = true;
        List<Claim> claims = new() { new Claim(ClaimTypes.Name, args.Username) };
        JwtSecurityToken jwt = new(
                issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        await Clients.Caller.SendAsync("ReceiveToken", encodedJwt, user);
    }
}
