using InventoryManager.API.DTOs.Common;
using InventoryManager.API.DTOs.TaskItem;

namespace InventoryManager.API.Services.Interfaces;

public interface ITaskItemService
{
    Task<ApiResponse<PagedResult<TaskItemDto>>> GetTasksAsync(TaskItemFilterParams filterParams);
    Task<ApiResponse<TaskItemDto>> GetTaskByIdAsync(int id);
    Task<ApiResponse<TaskItemDto>> CreateTaskAsync(CreateTaskItemDto dto);
    Task<ApiResponse<TaskItemDto>> UpdateTaskAsync(int id, UpdateTaskItemDto dto);
    Task<ApiResponse<bool>> DeleteTaskAsync(int id);
}