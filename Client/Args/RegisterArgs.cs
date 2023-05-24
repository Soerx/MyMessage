using Client.Models;
using System;

namespace Client.Args;

/// <summary>
/// Содержит пользовательские данные для регистрации
/// </summary>
public class RegisterArgs
{
    /// <param name="username">Логин пользователя</param>
    /// <param name="password">Пароль пользователя</param>
    /// <param name="firstname">Имя пользователя</param>
    /// <param name="lastname">Фамилия пользователя</param>
    /// <param name="birthdate">Дата рождения пользователя</param>
    /// <exception cref="ArgumentNullException"></exception>
    public RegisterArgs(string username, string password, string firstname, string lastname, Gender gender, DateTime birthdate)
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Password = password ?? throw new ArgumentNullException(nameof(password));
        Firstname = firstname ?? throw new ArgumentNullException(nameof(firstname));
        Lastname = lastname ?? throw new ArgumentNullException(nameof(lastname));
        Gender = gender;
        Birthdate = birthdate;
    }

    /// <summary>
    /// Поле логина пользователя
    /// </summary>
    public string Username { get; } = null!;

    /// <summary>
    /// Поле пароля пользователя
    /// </summary>
    public string Password { get; } = null!;

    /// <summary>
    /// Поле имени пользователя
    /// </summary>
    public string Firstname { get; } = null!;

    /// <summary>
    /// Поле фамилии пользователя
    /// </summary>
    public string Lastname { get; } = null!;

    /// <summary>
    /// Поле половой принадлежности пользователя
    /// </summary>
    public Gender Gender { get; }

    /// <summary>
    /// Поле даты рождения пользователя
    /// </summary>
    public DateTime Birthdate { get; }
}
