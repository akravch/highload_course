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
        var response = await _userService.LoginAsync(request);
        return response == null
            ? NotFound()
            : Ok(response);
    }

    [HttpPost("/user/register")]
    [ProducesResponseType(typeof(UserRegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [ServiceFilter<GlobalModelValidationFilter>]
    public async Task<ActionResult<UserRegisterResponse>> RegisterAsync([FromBody] UserRegisterRequest request)
    {
        return await _userService.RegisterAsync(request);
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
        var response = await _userService.GetAsync(id);
        return response != null
            ? Ok(response)
            : NotFound();
    }
}