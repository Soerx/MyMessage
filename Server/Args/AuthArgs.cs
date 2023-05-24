namespace Server.Args;

/// <summary>
/// Содержит пользовательские данные для авторизации
/// </summary>
public class AuthArgs
{
    /// <param name="username">Логин пользователя</param>
    /// <param name="password">Пароль пользователя</param>
    /// <exception cref="ArgumentNullException"></exception>
    public AuthArgs(string username, string password)
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Password = password ?? throw new ArgumentNullException(nameof(password));
    }

    /// <summary>
    /// Поле логина пользователя
    /// </summary>
    public string Username { get; } = null!;

    /// <summary>
    /// Поле пароля пользователя
    /// </summary>
    public string Password { get; } = null!;
}
