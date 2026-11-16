using InventoryManager.API.DTOs.Common;
using InventoryManager.API.DTOs.StockMovement;
using InventoryManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockMovementsController : ControllerBase
{
    private readonly IStockMovementService _movementService;

    public StockMovementsController(IStockMovementService movementService)
    {
        _movementService = movementService;
    }

    /// <summary>
    /// Retrieve the ledger movement history for a specific inventory item.
    /// Returns 404 (Anti-Discovery) if the parent item belongs to another user.
    /// </summary>
    [HttpGet("item/{inventoryItemId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StockMovementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StockMovementDto>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMovementsByItem(int inventoryItemId)
    {
        var result = await _movementService.GetMovementsByItemIdAsync(inventoryItemId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Record a new stock ledger movement (IN, OUT, or ADJUST).
    /// Enforces: Quantity &gt; 0 and OUT &lt;= Available Stock.
    /// Returns 404 (Anti-Discovery) if the item belongs to another user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StockMovementDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<StockMovementDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<StockMovementDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordMovement([FromBody] CreateStockMovementDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<StockMovementDto>.Fail("Validation failed.",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
        }

        var result = await _movementService.RecordMovementAsync(dto);
        if (!result.Success)
        {
            if (result.Message == "Inventory item not found.")
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetMovementsByItem), new { inventoryItemId = result.Data!.InventoryItemId }, result);
    }
}
