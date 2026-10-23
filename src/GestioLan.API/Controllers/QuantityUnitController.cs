using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GestioLan.API.Models;
using GestioLan.API.Services.QuantityUnits;

namespace GestioLan.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuantityUnitController : ControllerBase
{
    private readonly IQuantityUnitService _qtyUnitService;

    // Iniettiamo l'interfaccia del servizio, non il DbContext
    public QuantityUnitController(IQuantityUnitService qtyUnitService)
    {
        _qtyUnitService = qtyUnitService;
    }

    // Ottiene tutte le unità di misura dal DB
    [Authorize]
    [HttpGet("GetQtyUnits")]
    public async Task<ActionResult<IEnumerable<QuantityUnit>>> GetQtyUnits()
    {
        var units = await _qtyUnitService.GetQtyUnitsAsync();
        return Ok(units);
    }

    // Ottiene una singola unità di misura tramite il suo ID
    [Authorize]
    [HttpGet("GetQtyUnits/{id}")]
    public async Task<ActionResult<QuantityUnit>> GetQtyUnit(int id)
    {
        try
        {
            var unit = await _qtyUnitService.GetQtyUnitByIdAsync(id);
            return Ok(unit);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Quantity unit with ID {id} not found.");
        }
    }

    // Inserisce una nuova unità di misura
    [Authorize(Policy = "AdminOnly")] // Solo gli admin possono accedere a questo endpoint
    [HttpPost("AddQtyUnit")]
    public async Task<ActionResult<QuantityUnit>> AddQtyUnit([FromBody] QuantityUnit quantityUnit)
    {
        if (quantityUnit == null)
        {
            return BadRequest("Quantity unit invalid!");
        }

        try
        {
            var unit = await _qtyUnitService.CreateQtyUnitAsync(quantityUnit);
            // 201 Created con l'header Location verso la GET dell'unità
            return CreatedAtAction(nameof(GetQtyUnit), new { id = unit.IdQuantityUnit }, unit);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    // Aggiorna un'unità di misura
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("UpdateQtyUnit/{id}")]
    public async Task<IActionResult> UpdateQtyUnit(int id, [FromBody] QuantityUnit quantityUnit)
    {
        try
        {
            await _qtyUnitService.UpdateQtyUnitAsync(id, quantityUnit);
            return Ok("Quantity unit updated successfully");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Quantity unit with ID {id} not found.");
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    // Rimuove un'unità di misura (solo se non è usata da nessun item)
    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("DeleteQtyUnit/{id}")]
    public async Task<IActionResult> DeleteQtyUnit(int id)
    {
        try
        {
            await _qtyUnitService.DeleteQtyUnitAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Quantity unit with ID {id} not found.");
        }
        catch (InvalidOperationException ex)
        {
            // 409: l'unità è usata da alcuni item
            return Conflict(ex.Message);
        }
    }
}