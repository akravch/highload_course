using HighloadCourse.Models;
using Microsoft.AspNetCore.Mvc;

namespace HighloadCourse.Controllers;

[ApiController]
public sealed class UserController : ControllerBase
{
    [HttpPost("/login")]
    public async Task<UserLoginResponse> LoginAsync([FromBody] UserLoginRequest request)
    {
        return new UserLoginResponse { Token = "token" };
    }

    [HttpPost("/user/register")]
    public async Task<UserRegisterResponse> RegisterAsync([FromBody] UserRegisterRequest request)
    {
        return new UserRegisterResponse { UserId = "id" };
    }

    [HttpGet("/user/get/{id}")]
    public async Task<UserGetResponse> GetAsync([FromRoute] string id)
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