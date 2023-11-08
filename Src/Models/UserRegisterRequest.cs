namespace HighloadCourse.Models;

public sealed class UserRegisterRequest
{
    public required string FirstName { get; init; }
    public required string SecondName { get; init; }
    public required string Biography { get; init; }
    public required string City { get; init; }
    public required string Password { get; init; }
    public required DateOnly Birthdate { get; init; }
}