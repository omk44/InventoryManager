using InventoryManager.API.Data;
using InventoryManager.API.DTOs.Common;
using InventoryManager.API.DTOs.StockMovement;
using InventoryManager.API.Models.Entities;
using InventoryManager.API.Models.Enums;
using InventoryManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.API.Services.Implementations;

public class StockMovementService : IStockMovementService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public StockMovementService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Computes current stock dynamically from an ordered sequence of ledger movements.
    /// - IN: adds quantity
    /// - OUT: subtracts quantity
    /// - ADJUST: overrides/baselines the running stock to the physical count
    /// </summary>
    public static int ComputeStock(IEnumerable<StockMovement> movements)
    {
        int runningStock = 0;

        foreach (var movement in movements.OrderBy(m => m.CreatedAt).ThenBy(m => m.Id))
        {
            switch (movement.Type)
            {
                case StockMovementType.IN:
                    runningStock += movement.Quantity;
                    break;
                case StockMovementType.OUT:
                    runningStock -= movement.Quantity;
                    break;
                case StockMovementType.ADJUST:
                    runningStock = movement.Quantity; // Baseline override
                    break;
            }
        }

        return runningStock;
    }

    public async Task<ApiResponse<IEnumerable<StockMovementDto>>> GetMovementsByItemIdAsync(int inventoryItemId)
    {
        // 1. Anti-Discovery check on the parent InventoryItem
        var item = await _context.InventoryItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == inventoryItemId);

        if (item == null || (!_currentUserService.IsAdmin && item.OwnerUserId != _currentUserService.UserId))
        {
            return ApiResponse<IEnumerable<StockMovementDto>>.Fail("Inventory item not found.");
        }

        // 2. Fetch movements ordered chronologically (latest first for display)
        var movements = await _context.StockMovements
            .AsNoTracking()
            .Where(m => m.InventoryItemId == inventoryItemId)
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Select(m => new StockMovementDto
            {
                Id = m.Id,
                InventoryItemId = m.InventoryItemId,
                ItemName = m.InventoryItem != null ? m.InventoryItem.Name : string.Empty,
                SKU = m.InventoryItem != null ? m.InventoryItem.SKU : string.Empty,
                Type = m.Type,
                Quantity = m.Quantity,
                Note = m.Note,
                CreatedAt = m.CreatedAt,
                CreatedBy = m.CreatedBy
            })
            .ToListAsync();

        return ApiResponse<IEnumerable<StockMovementDto>>.Ok(movements);
    }

    public async Task<ApiResponse<StockMovementDto>> RecordMovementAsync(CreateStockMovementDto dto)
    {
        // 1. Validate Quantity > 0
        if (dto.Quantity <= 0)
        {
            return ApiResponse<StockMovementDto>.Fail("Movement quantity must be greater than zero.");
        }

        // 2. Anti-Discovery check on the parent InventoryItem
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == dto.InventoryItemId);

        if (item == null || (!_currentUserService.IsAdmin && item.OwnerUserId != _currentUserService.UserId))
        {
            return ApiResponse<StockMovementDto>.Fail("Inventory item not found.");
        }

        // 3. If movement is OUT, enforce rule: OUT <= Available Stock
        if (dto.Type == StockMovementType.OUT)
        {
            var allExistingMovements = await _context.StockMovements
                .AsNoTracking()
                .Where(m => m.InventoryItemId == dto.InventoryItemId)
                .ToListAsync();

            var currentStock = ComputeStock(allExistingMovements);

            if (dto.Quantity > currentStock)
            {
                return ApiResponse<StockMovementDto>.Fail(
                    $"Insufficient stock. Current available stock is {currentStock}, but requested OUT quantity is {dto.Quantity}."
                );
            }
        }

        // 4. Create and persist the movement
        var movement = new StockMovement
        {
            InventoryItemId = dto.InventoryItemId,
            Type = dto.Type,
            Quantity = dto.Quantity,
            Note = dto.Note?.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserName ?? "System"
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        var resultDto = new StockMovementDto
        {
            Id = movement.Id,
            InventoryItemId = movement.InventoryItemId,
            ItemName = item.Name,
            SKU = item.SKU,
            Type = movement.Type,
            Quantity = movement.Quantity,
            Note = movement.Note,
            CreatedAt = movement.CreatedAt,
            CreatedBy = movement.CreatedBy
        };

        return ApiResponse<StockMovementDto>.Ok(resultDto, "Stock movement recorded successfully.");
    }
}
