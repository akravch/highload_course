using HighloadCourse.ErrorHandling;
using HighloadCourse.Models;
using HighloadCourse.Services;
using Microsoft.AspNetCore.Mvc;

namespace HighloadCourse.Controllers;

[ApiController]
public sealed class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
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
        var result = await _userService.GetAsync(id);

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
}