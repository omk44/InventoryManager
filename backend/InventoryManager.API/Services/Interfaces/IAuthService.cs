using InventoryManager.API.DTOs.Auth;
using InventoryManager.API.DTOs.Common;

namespace InventoryManager.API.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<ApiResponse<bool>> LogoutAsync(string userId);
    Task<ApiResponse<UserProfileDto>> GetCurrentUserProfileAsync(string userId);
}
