using System.Security.Claims;
using InventoryManager.API.Configurations;
using InventoryManager.API.Data;
using InventoryManager.API.DTOs.Auth;
using InventoryManager.API.DTOs.Common;
using InventoryManager.API.Models.Entities;
using InventoryManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InventoryManager.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly ApplicationDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService,
        ApplicationDbContext dbContext,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ApiResponse<AuthResponseDto>.Fail("A user with this email address already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description);
            return ApiResponse<AuthResponseDto>.Fail("User registration failed.", errors);
        }

        // Ensure "User" role exists and assign
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }
        await _userManager.AddToRoleAsync(user, "User");

        var responseDto = await GenerateAuthResponseForUserAsync(user);
        return ApiResponse<AuthResponseDto>.Ok(responseDto, "User registered successfully.");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return ApiResponse<AuthResponseDto>.Fail("Invalid email or password.");
        }

        var responseDto = await GenerateAuthResponseForUserAsync(user);
        return ApiResponse<AuthResponseDto>.Ok(responseDto, "Login successful.");
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        ClaimsPrincipal? principal;
        try
        {
            principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        }
        catch
        {
            return ApiResponse<AuthResponseDto>.Fail("Invalid access token.");
        }

        if (principal == null)
        {
            return ApiResponse<AuthResponseDto>.Fail("Invalid access token.");
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponse<AuthResponseDto>.Fail("Invalid token claims.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<AuthResponseDto>.Fail("User no longer exists.");
        }

        var existingRefreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken && r.UserId == userId);

        if (existingRefreshToken == null || !existingRefreshToken.IsActive)
        {
            return ApiResponse<AuthResponseDto>.Fail("Refresh token is invalid or expired.");
        }

        // Revoke the old refresh token (Token Rotation)
        existingRefreshToken.IsRevoked = true;
        existingRefreshToken.RevokedAtUtc = DateTime.UtcNow;

        var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);
        existingRefreshToken.ReplacedByToken = newRefreshToken.Token;

        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync();

        var newAccessToken = await _tokenService.GenerateAccessTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        var response = new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            Roles = roles
        };

        return ApiResponse<AuthResponseDto>.Ok(response, "Token refreshed successfully.");
    }

    public async Task<ApiResponse<bool>> LogoutAsync(string userId)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Logged out successfully.");
    }

    public async Task<ApiResponse<UserProfileDto>> GetCurrentUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<UserProfileDto>.Fail("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var profile = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            Roles = roles,
            CreatedAt = user.CreatedAt
        };

        return ApiResponse<UserProfileDto>.Ok(profile);
    }

    private async Task<AuthResponseDto> GenerateAuthResponseForUserAsync(ApplicationUser user)
    {
        var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            Roles = roles
        };
    }
}
