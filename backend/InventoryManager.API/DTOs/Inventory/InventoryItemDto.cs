namespace InventoryManager.API.DTOs.Inventory;

public class InventoryItemDto
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int ReorderLevel { get; set; }
    public int CurrentStock { get; set; } // Computed dynamically server-side!
    public bool IsLowStock => CurrentStock <= ReorderLevel; // Computed alert flag
    public string OwnerUserId { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}