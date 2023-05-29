using Server.Models;

namespace Server.Args;

public class RegisterArgs
{
    public RegisterArgs(string username, string password, string firstname, string lastname, Gender gender, DateTime birthdate)
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Password = password ?? throw new ArgumentNullException(nameof(password));
        Firstname = firstname ?? throw new ArgumentNullException(nameof(firstname));
        Lastname = lastname ?? throw new ArgumentNullException(nameof(lastname));
        Gender = gender;
        Birthdate = birthdate;
    }

    public string Username { get; } = null!;
    public string Password { get; } = null!;
    public string Firstname { get; } = null!;
    public string Lastname { get; } = null!;
    public Gender Gender { get; }
    public DateTime Birthdate { get; }
}
