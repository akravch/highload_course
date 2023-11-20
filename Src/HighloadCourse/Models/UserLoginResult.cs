namespace HighloadCourse.Models;

public sealed class UserLoginResult
{
    private UserLoginResult(string? token, UserLoginError error)
    {
        Token = token;
        Error = error;
    }

    public string? Token { get; }
    public UserLoginError Error { get; }

    public static UserLoginResult NotFound { get; } = new(null, UserLoginError.UserNotFound);
    public static UserLoginResult InvalidPassword { get; } = new(null, UserLoginError.InvalidPassword);
    public static UserLoginResult Success(string token) => new(token, UserLoginError.None);
}