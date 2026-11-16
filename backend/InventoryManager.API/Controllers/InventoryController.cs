using InventoryManager.API.DTOs.Common;
using InventoryManager.API.DTOs.Inventory;
using InventoryManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// Retrieve paginated and filtered inventory items.
    /// Standard users see only their items; Admins see all items across the system.
    /// Stock quantity is dynamically computed from ledger movements.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InventoryItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] InventoryItemFilterParams filterParams)
    {
        var result = await _inventoryService.GetItemsAsync(filterParams);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve a single inventory item by Id.
    /// Returns 404 (Anti-Discovery) if the item belongs to another user.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _inventoryService.GetItemByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new inventory item with optional initial stock intake.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<InventoryItemDto>.Fail("Validation failed.",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
        }

        var result = await _inventoryService.CreateItemAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing inventory item.
    /// Returns 404 (Anti-Discovery) if the item belongs to another user.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInventoryItemDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<InventoryItemDto>.Fail("Validation failed.",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
        }

        var result = await _inventoryService.UpdateItemAsync(id, dto);
        if (!result.Success)
        {
            if (result.Message == "Inventory item not found.")
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete an inventory item and all its ledger history.
    /// Returns 404 (Anti-Discovery) if the item belongs to another user.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _inventoryService.DeleteItemAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieve the live dynamically computed stock balance for an item.
    /// </summary>
    [HttpGet("{id:int}/stock")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStockBalance(int id)
    {
        var itemCheck = await _inventoryService.GetItemByIdAsync(id);
        if (!itemCheck.Success)
        {
            return NotFound(ApiResponse<int>.Fail("Inventory item not found."));
        }

        var stock = await _inventoryService.GetCurrentStockAsync(id);
        return Ok(ApiResponse<int>.Ok(stock));
    }
}
