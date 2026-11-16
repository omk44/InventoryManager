using InventoryManager.API.Data;
using InventoryManager.API.DTOs.Common;
using InventoryManager.API.DTOs.TaskItem;
using InventoryManager.API.Models.Entities;
using InventoryManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.API.Services.Implementations;

public class TaskItemService : ITaskItemService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public TaskItemService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Enforces Ownership Scoping:
    /// - Admin has access to all records.
    /// - Standard user only has access to their own records.
    /// - Trying to access another user's record filters it out, leading to 404 (Anti-Discovery).
    /// </summary>
    private IQueryable<TaskItem> GetScopedTaskQuery()
    {
        var query = _context.TaskItems.AsQueryable();

        if (!_currentUserService.IsAdmin)
        {
            var currentUserId = _currentUserService.UserId ?? string.Empty;
            query = query.Where(t => t.OwnerUserId == currentUserId);
        }

        return query;
    }

    public async Task<ApiResponse<PagedResult<TaskItemDto>>> GetTasksAsync(TaskItemFilterParams filterParams)
    {
        var query = GetScopedTaskQuery().AsNoTracking();

        // 1. Text search by Title
        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            var search = filterParams.SearchTerm.Trim().ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(search));
        }

        // 2. Filter by Status
        if (filterParams.Status.HasValue)
        {
            query = query.Where(t => t.Status == filterParams.Status.Value);
        }

        // 3. Filter by Priority
        if (filterParams.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filterParams.Priority.Value);
        }

        // 4. Count total matching records before pagination
        var totalCount = await query.CountAsync();

        // 5. Apply ordering, pagination, and projection
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(t => new TaskItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DueDate = t.DueDate,
                Status = t.Status,
                Priority = t.Priority,
                OwnerUserId = t.OwnerUserId,
                OwnerName = t.OwnerUser != null ? (t.OwnerUser.FullName ?? t.OwnerUser.UserName) : null,
                CreatedAt = t.CreatedAt,
                CreatedBy = t.CreatedBy,
                UpdatedAt = t.UpdatedAt,
                UpdatedBy = t.UpdatedBy
            })
            .ToListAsync();

        var pagedResult = new PagedResult<TaskItemDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
        return ApiResponse<PagedResult<TaskItemDto>>.Ok(pagedResult);
    }

    public async Task<ApiResponse<TaskItemDto>> GetTaskByIdAsync(int id)
    {
        var task = await GetScopedTaskQuery()
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TaskItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DueDate = t.DueDate,
                Status = t.Status,
                Priority = t.Priority,
                OwnerUserId = t.OwnerUserId,
                OwnerName = t.OwnerUser != null ? (t.OwnerUser.FullName ?? t.OwnerUser.UserName) : null,
                CreatedAt = t.CreatedAt,
                CreatedBy = t.CreatedBy,
                UpdatedAt = t.UpdatedAt,
                UpdatedBy = t.UpdatedBy
            })
            .FirstOrDefaultAsync();

        // Anti-Discovery Rule: Return 404 if record doesn't exist or is not owned by user
        if (task == null)
        {
            return ApiResponse<TaskItemDto>.Fail("Task not found.");
        }

        return ApiResponse<TaskItemDto>.Ok(task);
    }

    public async Task<ApiResponse<TaskItemDto>> CreateTaskAsync(CreateTaskItemDto dto)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return ApiResponse<TaskItemDto>.Fail("User is not authenticated.");
        }

        var currentUserName = _currentUserService.UserName ?? "System";

        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            DueDate = dto.DueDate,
            Status = dto.Status,
            Priority = dto.Priority,
            OwnerUserId = currentUserId, // Server-stamped ownership
            CreatedAt = DateTime.UtcNow,  // Server-stamped audit
            CreatedBy = currentUserName  // Server-stamped audit
        };

        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();

        var resultDto = new TaskItemDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            Status = task.Status,
            Priority = task.Priority,
            OwnerUserId = task.OwnerUserId,
            OwnerName = currentUserName,
            CreatedAt = task.CreatedAt,
            CreatedBy = task.CreatedBy
        };

        return ApiResponse<TaskItemDto>.Ok(resultDto, "Task created successfully.");
    }

    public async Task<ApiResponse<TaskItemDto>> UpdateTaskAsync(int id, UpdateTaskItemDto dto)
    {
        // Anti-Discovery: GetScopedTaskQuery ensures user only retrieves their own task
        var task = await GetScopedTaskQuery().FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            return ApiResponse<TaskItemDto>.Fail("Task not found.");
        }

        task.Title = dto.Title.Trim();
        task.Description = dto.Description?.Trim();
        task.DueDate = dto.DueDate;
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        task.UpdatedAt = DateTime.UtcNow; // Server-stamped audit
        task.UpdatedBy = _currentUserService.UserName ?? "System";

        await _context.SaveChangesAsync();

        var resultDto = new TaskItemDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            Status = task.Status,
            Priority = task.Priority,
            OwnerUserId = task.OwnerUserId,
            CreatedAt = task.CreatedAt,
            CreatedBy = task.CreatedBy,
            UpdatedAt = task.UpdatedAt,
            UpdatedBy = task.UpdatedBy
        };

        return ApiResponse<TaskItemDto>.Ok(resultDto, "Task updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteTaskAsync(int id)
    {
        // Anti-Discovery: GetScopedTaskQuery ensures user only retrieves their own task
        var task = await GetScopedTaskQuery().FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            return ApiResponse<bool>.Fail("Task not found.");
        }

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Task deleted successfully.");
    }
}
