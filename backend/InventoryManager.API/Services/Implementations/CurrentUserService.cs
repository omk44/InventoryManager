using System.Security.Claims;
using InventoryManager.API.Services.Interfaces;

namespace InventoryManager.API.Services.Implementations;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName => User?.FindFirstValue(ClaimTypes.Name) 
        ?? User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAdmin => User?.IsInRole("Admin") ?? false;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
