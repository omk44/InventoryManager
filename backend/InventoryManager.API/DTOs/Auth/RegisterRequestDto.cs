using System.ComponentModel.DataAnnotations;

namespace InventoryManager.API.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
}
