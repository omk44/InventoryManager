using System.ComponentModel.DataAnnotations;

namespace InventoryManager.API.Models.Entities;

public class RefreshToken
{
    public int Id { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresOnUtc { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAtUtc { get; set; }

    public string? ReplacedByToken { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresOnUtc;
    public bool IsActive => !IsRevoked && !IsExpired;
}
