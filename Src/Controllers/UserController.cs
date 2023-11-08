using HighloadCourse.Models;
using Microsoft.AspNetCore.Mvc;

namespace HighloadCourse.Controllers;

[ApiController]
public sealed class UserController : ControllerBase
{
    [HttpPost("/login")]
    [ProducesResponseType(typeof(UserLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<UserLoginResponse> LoginAsync([FromBody] UserLoginRequest request)
    {
        return new UserLoginResponse { Token = "token" };
    }

    [HttpPost("/user/register")]
    [ProducesResponseType(typeof(UserRegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<UserRegisterResponse> RegisterAsync([FromBody] UserRegisterRequest request)
    {
        return new UserRegisterResponse { UserId = "id" };
    }

    [HttpGet("/user/get/{id}")]
    [ProducesResponseType(typeof(UserGetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
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