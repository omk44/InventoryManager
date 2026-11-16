using System.ComponentModel.DataAnnotations;
using InventoryManager.API.Models.Enums;

namespace InventoryManager.API.DTOs.TaskItem;

public class UpdateTaskItemDto
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public TaskItemStatus Status { get; set; }

    public TaskPriority Priority { get; set; }
}