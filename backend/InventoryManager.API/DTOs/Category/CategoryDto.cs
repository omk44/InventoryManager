namespace InventoryManager.API.DTOs.Category;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public int ItemCount { get; set; } // Nice bonus for UI so users see how many items are in this category!
}