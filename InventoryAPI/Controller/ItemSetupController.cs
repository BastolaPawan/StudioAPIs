using InventoryAPI.DTOs;
using InventoryAPI.Models;
using InventoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryAPI.Controller;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Apply to entire controller if all endpoints need auth
public class ItemSetupController : ControllerBase
{
    private readonly IItemRepository _itemRepository;

    public ItemSetupController(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    // GET: api/itemsetup/GetAllItems?searchTerm=pawan&pageNumber=1&pageSize=35
    [HttpGet("GetAllItems")]
    public async Task<ActionResult<IEnumerable<Item>>> GetItems(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 35)
    {
        var result = await _itemRepository.GetAllAsync(searchTerm, pageNumber, pageSize);

        return Ok(result); // Always return 200 with data (empty array if no items)
    }

    // GET: api/itemsetup/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Item>> GetItemById(int id)
    {
        var item = await _itemRepository.GetByIdAsync(id);

        if (item == null)
            return NotFound(new { message = $"Item with ID {id} not found" });

        return Ok(item);
    }

    // POST: api/itemsetup
    [HttpPost]
    public async Task<ActionResult<Item>> CreateItem([FromBody] ItemMasterDto itemDto)
    {
        // Check for duplicate ItemCode
        var existingItem = await _itemRepository.GetByCodeAsync(itemDto.ItemCode);
        if (existingItem != null)
            return Conflict(new { message = $"Item with code '{itemDto.ItemCode}' already exists" });

        var item = new Item
        {
            ItemCode = itemDto.ItemCode,
            Name = itemDto.Name,
            Price = itemDto.Price,
            ItemGroupId = itemDto.ItemGroupId,
            UnitOfMeasureId = itemDto.UnitOfMeasureId
        };

        await _itemRepository.AddAsync(item);

        if (!await _itemRepository.SaveChangesAsync())
            return StatusCode(500, new { message = "Failed to create item" });

        return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, item);
    }

    // PUT: api/itemsetup/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateItem(int id, [FromBody] ItemMasterDto itemDto)
    {
        var existingItem = await _itemRepository.GetByIdAsync(id);
        if (existingItem == null)
            return NotFound(new { message = $"Item with ID {id} not found" });

        // Check if new ItemCode conflicts with another item
        if (existingItem.ItemCode != itemDto.ItemCode)
        {
            var codeExists = await _itemRepository.GetByCodeAsync(itemDto.ItemCode);
            if (codeExists != null && codeExists.Id != id)
                return Conflict(new { message = $"Item with code '{itemDto.ItemCode}' already exists" });
        }

        existingItem.ItemCode = itemDto.ItemCode;
        existingItem.Name = itemDto.Name;
        existingItem.Price = itemDto.Price;
        existingItem.ItemGroupId = itemDto.ItemGroupId;
        existingItem.UnitOfMeasureId = itemDto.UnitOfMeasureId;

        await _itemRepository.UpdateAsync(existingItem);

        if (!await _itemRepository.SaveChangesAsync())
            return StatusCode(500, new { message = "Failed to update item" });

        return NoContent(); // 204 - successful update
    }

    // DELETE: api/itemsetup/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteItem(int id)
    {
        var item = await _itemRepository.GetByIdAsync(id);
        if (item == null)
            return NotFound(new { message = $"Item with ID {id} not found" });

        await _itemRepository.DeleteAsync(id);

        if (!await _itemRepository.SaveChangesAsync())
            return StatusCode(500, new { message = "Failed to delete item" });

        return NoContent(); // 204 - successful deletion
    }
}