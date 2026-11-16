using InventoryManager.API.DTOs.Common;
using InventoryManager.API.Models.Enums;

namespace InventoryManager.API.DTOs.TaskItem;

public class TaskItemFilterParams : PaginationParams
{
    public TaskItemStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
}