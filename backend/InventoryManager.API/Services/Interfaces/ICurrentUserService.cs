namespace InventoryManager.API.Services.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    bool IsAdmin { get; }
    bool IsAuthenticated { get; }
}
