using InventoryManager.API.Data;
using InventoryManager.API.DTOs.Common;
using InventoryManager.API.DTOs.Inventory;
using InventoryManager.API.Models.Entities;
using InventoryManager.API.Models.Enums;
using InventoryManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.API.Services.Implementations;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public InventoryService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Enforces Ownership Scoping:
    /// - Admin has access to all inventory items.
    /// - Standard users only see/edit their own items.
    /// - Cross-user access filters out records, resulting in 404 (Anti-Discovery Rule).
    /// </summary>
    private IQueryable<InventoryItem> GetScopedInventoryQuery()
    {
        var query = _context.InventoryItems.AsQueryable();

        if (!_currentUserService.IsAdmin)
        {
            var currentUserId = _currentUserService.UserId ?? string.Empty;
            query = query.Where(i => i.OwnerUserId == currentUserId);
        }

        return query;
    }

    public async Task<ApiResponse<PagedResult<InventoryItemDto>>> GetItemsAsync(InventoryItemFilterParams filterParams)
    {
        var query = GetScopedInventoryQuery()
            .Include(i => i.Category)
            .Include(i => i.OwnerUser)
            .Include(i => i.StockMovements)
            .AsNoTracking();

        // 1. Search by Name or SKU
        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            var search = filterParams.SearchTerm.Trim().ToLower();
            query = query.Where(i => i.Name.ToLower().Contains(search) || i.SKU.ToLower().Contains(search));
        }

        // 2. Filter by Category
        if (filterParams.CategoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == filterParams.CategoryId.Value);
        }

        var allMatchingItems = await query.ToListAsync();

        // 3. Compute dynamic stock for each item
        var itemDtos = allMatchingItems.Select(item =>
        {
            var currentStock = StockMovementService.ComputeStock(item.StockMovements);

            return new InventoryItemDto
            {
                Id = item.Id,
                SKU = item.SKU,
                Name = item.Name,
                Description = item.Description,
                CategoryId = item.CategoryId,
                CategoryName = item.Category?.Name,
                ReorderLevel = item.ReorderLevel,
                CurrentStock = currentStock,
                OwnerUserId = item.OwnerUserId,
                OwnerName = item.OwnerUser != null ? (item.OwnerUser.FullName ?? item.OwnerUser.UserName) : null,
                CreatedAt = item.CreatedAt,
                CreatedBy = item.CreatedBy,
                UpdatedAt = item.UpdatedAt,
                UpdatedBy = item.UpdatedBy
            };
        });

        // 4. Low Stock Filter (computed server-side based on ledger)
        if (filterParams.LowStockOnly == true)
        {
            itemDtos = itemDtos.Where(i => i.IsLowStock);
        }

        var dtoList = itemDtos.OrderBy(i => i.Name).ToList();
        var totalCount = dtoList.Count;

        // 5. Pagination
        var pagedItems = dtoList
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .ToList();

        var result = new PagedResult<InventoryItemDto>(pagedItems, totalCount, filterParams.PageNumber, filterParams.PageSize);
        return ApiResponse<PagedResult<InventoryItemDto>>.Ok(result);
    }

    public async Task<ApiResponse<InventoryItemDto>> GetItemByIdAsync(int id)
    {
        var item = await GetScopedInventoryQuery()
            .Include(i => i.Category)
            .Include(i => i.OwnerUser)
            .Include(i => i.StockMovements)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        // Anti-Discovery: Return 404 if item does not exist or belongs to another user
        if (item == null)
        {
            return ApiResponse<InventoryItemDto>.Fail("Inventory item not found.");
        }

        var currentStock = StockMovementService.ComputeStock(item.StockMovements);

        var dto = new InventoryItemDto
        {
            Id = item.Id,
            SKU = item.SKU,
            Name = item.Name,
            Description = item.Description,
            CategoryId = item.CategoryId,
            CategoryName = item.Category?.Name,
            ReorderLevel = item.ReorderLevel,
            CurrentStock = currentStock,
            OwnerUserId = item.OwnerUserId,
            OwnerName = item.OwnerUser != null ? (item.OwnerUser.FullName ?? item.OwnerUser.UserName) : null,
            CreatedAt = item.CreatedAt,
            CreatedBy = item.CreatedBy,
            UpdatedAt = item.UpdatedAt,
            UpdatedBy = item.UpdatedBy
        };

        return ApiResponse<InventoryItemDto>.Ok(dto);
    }

    public async Task<ApiResponse<InventoryItemDto>> CreateItemAsync(CreateInventoryItemDto dto)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return ApiResponse<InventoryItemDto>.Fail("User is not authenticated.");
        }

        // 1. Verify Category exists
        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null)
        {
            return ApiResponse<InventoryItemDto>.Fail("Specified category does not exist.");
        }

        // 2. Validate SKU uniqueness for this owner
        var normalizedSku = dto.SKU.Trim();
        var skuExists = await _context.InventoryItems
            .AnyAsync(i => i.OwnerUserId == currentUserId && i.SKU.ToLower() == normalizedSku.ToLower());

        if (skuExists)
        {
            return ApiResponse<InventoryItemDto>.Fail($"An item with SKU '{normalizedSku}' already exists in your inventory.");
        }

        var currentUserName = _currentUserService.UserName ?? "System";

        var item = new InventoryItem
        {
            SKU = normalizedSku,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            CategoryId = dto.CategoryId,
            ReorderLevel = dto.ReorderLevel,
            OwnerUserId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserName
        };

        // 3. Handle optional initial stock intake as first ledger entry
        if (dto.InitialStock > 0)
        {
            item.StockMovements.Add(new StockMovement
            {
                Type = StockMovementType.IN,
                Quantity = dto.InitialStock,
                Note = "Initial stock intake on creation.",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserName
            });
        }

        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();

        var resultDto = new InventoryItemDto
        {
            Id = item.Id,
            SKU = item.SKU,
            Name = item.Name,
            Description = item.Description,
            CategoryId = item.CategoryId,
            CategoryName = category.Name,
            ReorderLevel = item.ReorderLevel,
            CurrentStock = dto.InitialStock,
            OwnerUserId = item.OwnerUserId,
            OwnerName = currentUserName,
            CreatedAt = item.CreatedAt,
            CreatedBy = item.CreatedBy
        };

        return ApiResponse<InventoryItemDto>.Ok(resultDto, "Inventory item created successfully.");
    }

    public async Task<ApiResponse<InventoryItemDto>> UpdateItemAsync(int id, UpdateInventoryItemDto dto)
    {
        // Anti-Discovery: Scoped query ensures user only updates their own item
        var item = await GetScopedInventoryQuery()
            .Include(i => i.Category)
            .Include(i => i.StockMovements)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
        {
            return ApiResponse<InventoryItemDto>.Fail("Inventory item not found.");
        }

        // 1. Verify Category exists
        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null)
        {
            return ApiResponse<InventoryItemDto>.Fail("Specified category does not exist.");
        }

        // 2. Validate SKU uniqueness for this owner (excluding this item)
        var normalizedSku = dto.SKU.Trim();
        var skuDuplicate = await _context.InventoryItems
            .AnyAsync(i => i.Id != id && i.OwnerUserId == item.OwnerUserId && i.SKU.ToLower() == normalizedSku.ToLower());

        if (skuDuplicate)
        {
            return ApiResponse<InventoryItemDto>.Fail($"Another item with SKU '{normalizedSku}' already exists in your inventory.");
        }

        var currentUserName = _currentUserService.UserName ?? "System";

        item.SKU = normalizedSku;
        item.Name = dto.Name.Trim();
        item.Description = dto.Description?.Trim();
        item.CategoryId = dto.CategoryId;
        item.ReorderLevel = dto.ReorderLevel;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = currentUserName;

        await _context.SaveChangesAsync();

        var currentStock = StockMovementService.ComputeStock(item.StockMovements);

        var resultDto = new InventoryItemDto
        {
            Id = item.Id,
            SKU = item.SKU,
            Name = item.Name,
            Description = item.Description,
            CategoryId = item.CategoryId,
            CategoryName = category.Name,
            ReorderLevel = item.ReorderLevel,
            CurrentStock = currentStock,
            OwnerUserId = item.OwnerUserId,
            CreatedAt = item.CreatedAt,
            CreatedBy = item.CreatedBy,
            UpdatedAt = item.UpdatedAt,
            UpdatedBy = item.UpdatedBy
        };

        return ApiResponse<InventoryItemDto>.Ok(resultDto, "Inventory item updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteItemAsync(int id)
    {
        // Anti-Discovery: Scoped query ensures user only deletes their own item
        var item = await GetScopedInventoryQuery().FirstOrDefaultAsync(i => i.Id == id);
        if (item == null)
        {
            return ApiResponse<bool>.Fail("Inventory item not found.");
        }

        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Inventory item and associated movements deleted successfully.");
    }

    public async Task<int> GetCurrentStockAsync(int inventoryItemId)
    {
        var movements = await _context.StockMovements
            .AsNoTracking()
            .Where(m => m.InventoryItemId == inventoryItemId)
            .ToListAsync();

        return StockMovementService.ComputeStock(movements);
    }
}
