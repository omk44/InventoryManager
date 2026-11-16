using System.ComponentModel.DataAnnotations;

namespace InventoryManager.API.DTOs.Inventory;

public class UpdateInventoryItemDto
{
    [Required(ErrorMessage = "SKU is required.")]
    [MaxLength(100, ErrorMessage = "SKU cannot exceed 100 characters.")]
    public string SKU { get; set; } = string.Empty;

    [Required(ErrorMessage = "Item name is required.")]
    [MaxLength(200, ErrorMessage = "Item name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "CategoryId is required.")]
    public int CategoryId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Reorder level must be 0 or greater.")]
    public int ReorderLevel { get; set; } = 0;
}