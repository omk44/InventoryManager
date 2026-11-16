using System.Security.Claims;
using InventoryManager.API.DTOs.Auth;
using InventoryManager.API.DTOs.Common;
using InventoryManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Validation error.", 
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
        }

        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Authenticate user credentials and issue JWT + Refresh tokens.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Validation error.",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
        }

        var result = await _authService.LoginAsync(request);
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Exchange an expired JWT access token and active refresh token for a fresh pair.
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Validation error.",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
        }

        var result = await _authService.RefreshTokenAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Log out the currently authenticated user and revoke active refresh tokens.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<bool>.Fail("User is not authenticated."));
        }

        var result = await _authService.LogoutAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve profile information and assigned roles for the currently logged-in user.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<UserProfileDto>.Fail("User is not authenticated."));
        }

        var result = await _authService.GetCurrentUserProfileAsync(userId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
