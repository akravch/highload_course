namespace HighloadCourse.Models;

public sealed class UserGetResult
{
    public UserGetResult(string id, string firstName, string secondName, string biography, string city, DateOnly birthdate)
    {
        Id = id;
        FirstName = firstName;
        SecondName = secondName;
        Biography = biography;
        City = city;
        Birthdate = birthdate;
    }

    public string Id { get; }
    public string FirstName { get; }
    public string SecondName { get; }
    public string Biography { get; }
    public string City { get; }
    public DateOnly Birthdate { get; }
}