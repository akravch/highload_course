using HighloadCourse.ErrorHandling;
using HighloadCourse.Models;
using HighloadCourse.Services;
using Microsoft.AspNetCore.Mvc;

namespace HighloadCourse.Controllers;

[ApiController]
public sealed class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly UserServiceReadOnly _userServiceReadOnly;

    public UserController(UserService userService, UserServiceReadOnly userServiceReadOnly)
    {
        _userService = userService;
        _userServiceReadOnly = userServiceReadOnly;
    }

    [HttpPost("/login")]
    [ProducesResponseType(typeof(UserLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [ServiceFilter<GlobalModelValidationFilter>]
    public async Task<ActionResult<UserLoginResponse>> LoginAsync([FromBody] UserLoginRequest request)
    {
        var result = await _userService.LoginAsync(request);

        switch (result.Error)
        {
            case UserLoginError.UserNotFound:
                return NotFound();
            case UserLoginError.InvalidPassword:
                return BadRequest();
            default:
                return Ok(new UserLoginResponse { Token = result.Token! });
        }
    }

    [HttpPost("/user/register")]
    [ProducesResponseType(typeof(UserRegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [ServiceFilter<GlobalModelValidationFilter>]
    public async Task<ActionResult<UserRegisterResponse>> RegisterAsync([FromBody] UserRegisterRequest request)
    {
        var userId = await _userService.RegisterAsync(request);

        return Ok(new UserRegisterResponse { UserId = userId });
    }

    [HttpGet("/user/get/{id}")]
    [ProducesResponseType(typeof(UserGetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [ServiceFilter<GlobalModelValidationFilter>]
    public async Task<ActionResult<UserGetResponse>> GetAsync([FromRoute] string id)
    {
        var result = await _userServiceReadOnly.GetAsync(id);

        return result != null
            ? Ok(new UserGetResponse
            {
                Id = result.Id,
                FirstName = result.FirstName,
                SecondName = result.SecondName,
                Biography = result.Biography,
                City = result.City,
                Birthdate = result.Birthdate
            })
            : NotFound();
    }

    [HttpGet("/user/search")]
    [ProducesResponseType(typeof(UserGetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<UserGetResponse[]>> SearchAsync([FromQuery(Name = "first_name")] string firstName, [FromQuery(Name = "last_name")] string lastName)
    {
        var users = await _userServiceReadOnly.SearchAsync(firstName, lastName);
        var usersCount = users.Count;
        var responses = new UserGetResponse[usersCount];

        for (var i = 0; i < usersCount; i++)
        {
            var user = users[i];
            responses[i] = new UserGetResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                SecondName = user.SecondName,
                Biography = user.Biography,
                City = user.City,
                Birthdate = user.Birthdate
            };
        }

        return Ok(responses);
    }
}