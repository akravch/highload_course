namespace HighloadCourse.Models;

public sealed class UserLoginRequest
{
    public required string Id { get; init; }
    public required string Password { get; init; }
}