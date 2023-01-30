using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Models.Users;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
  private readonly IAuthManager _authManager;
  private readonly ILogger<AccountController> _logger;

  public AccountController(
    IAuthManager authManager,
    ILogger<AccountController> logger
  )
  {
    _authManager = authManager;
    _logger = logger;
  }

  // POST: api/account/register
  [HttpPost]
  [Route("register")]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> Register([FromBody] ApiUserDto apiUserDto)
  {
    _logger.LogInformation($"Registration Attempt for {apiUserDto.Email}");
    try
    {
      var errors = await _authManager.Register(apiUserDto);

      if (errors.Any())
      {
        foreach (var error in errors)
          ModelState.AddModelError(error.Code, error.Description);

        return BadRequest(ModelState);
      }

      return Ok();
    }
    catch (Exception e)
    {
      _logger.LogError(
        e,
        $"Something Went Wrong in the {nameof(Register)} - User Registration Attempt for {apiUserDto.Email}"
      );
      return Problem(
        $"Something Went Wrong in the {nameof(Register)}. Please contact support",
        statusCode: 500
      );
    }
  }

  // POST: api/account/login
  [HttpPost]
  [Route("login")]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
  {
    _logger.LogInformation($"Login Attempt for {loginDto.Email}");
    try
    {
      var authResponse = await _authManager.Login(loginDto);

      if (authResponse == null) return Unauthorized();

      return Ok(authResponse);
    }
    catch (Exception e)
    {
      _logger.LogError(
        e,
        $"Something Went Wrong in the {nameof(Login)} - User Login Attempt for {loginDto.Email}"
      );
      return Problem(
        $"Something Went Wrong in the {nameof(Login)}. Please contact support",
        statusCode: 500
      );
    }
  }

  // POST: api/account/refreshtoken
  [HttpPost]
  [Route("refreshtoken")]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> RefreshToken(
    [FromBody] AuthResponseDto request
  )
  {
    var authResponse = await _authManager.VerifyRefreshToken(request);

    if (authResponse == null) return Unauthorized();

    return Ok(authResponse);
  }
}
