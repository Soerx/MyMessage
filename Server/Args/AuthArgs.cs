namespace Server.Args;

public class AuthArgs
{
    public AuthArgs(string username, string password)
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Password = password ?? throw new ArgumentNullException(nameof(password));
    }

    public string Username { get; } = null!;
    public string Password { get; } = null!;
}
