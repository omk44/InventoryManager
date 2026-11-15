using System.ComponentModel.DataAnnotations;
using InventoryManager.API.Models.Common;
using InventoryManager.API.Models.Enums;

namespace InventoryManager.API.Models.Entities;

public class TaskItem : BaseAuditableEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [Required]
    public string OwnerUserId { get; set; } = string.Empty;

    public ApplicationUser? OwnerUser { get; set; }
}
