using System.Security.Claims;
using InventoryManager.API.Models.Entities;

namespace InventoryManager.API.Services.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(ApplicationUser user);
    RefreshToken GenerateRefreshToken(string userId);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
