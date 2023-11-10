using HighloadCourse.Models;

namespace HighloadCourse.Services;

public sealed class UserService
{
    public async Task<UserLoginResponse> LoginAsync(UserLoginRequest request)
    {
        return new UserLoginResponse { Token = "token" };

    }

    public async Task<UserRegisterResponse> RegisterAsync(UserRegisterRequest request)
    {
        return new UserRegisterResponse { UserId = "id" };

    }

    public async Task<UserGetResponse> GetAsync(string id)
    {
        return new UserGetResponse
        {
            Id = "id",
            FirstName = "first",
            SecondName = "second",
            Biography = "bio",
            City = "city",
            Birthdate = DateTimeOffset.Now
        };

    }
}