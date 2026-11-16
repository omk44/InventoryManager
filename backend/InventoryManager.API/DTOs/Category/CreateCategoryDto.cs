using System.ComponentModel.DataAnnotations;

namespace InventoryManager.API.DTOs.Category;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;
}