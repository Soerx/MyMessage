
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Server.Models;

namespace Server.Tools;

public class TokenGenerator
{
    public static string GetNewToken(User user)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        List<Claim> claims = new() { new Claim(ClaimTypes.Name, user.Username) };
        JwtSecurityToken jwt = new(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}