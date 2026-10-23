using Microsoft.EntityFrameworkCore;
using GestioLan.API.Models;

namespace GestioLan.API.Services.QuantityUnits;

public class QuantityUnitService : IQuantityUnitService
{
    private readonly GestioLanContext _context;
    private readonly ILogger<QuantityUnitService> _logger;

    public QuantityUnitService(GestioLanContext context, ILogger<QuantityUnitService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Ottiene la lista di tutte le unità di misura, ordinate per nome
    public async Task<IEnumerable<QuantityUnit>> GetQtyUnitsAsync()
    {
        _logger.LogInformation("Retrieving all quantity units");

        var units = await _context.QuantityUnits
            .AsNoTracking()                 // sola lettura: nessun tracking necessario
            .OrderBy(u => u.Name)
            .ToListAsync();

        _logger.LogInformation("Returned {Count} quantity units", units.Count);
        return units;
    }

    // Ottiene la singola unità tramite il suo ID
    public async Task<QuantityUnit> GetQtyUnitByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving quantity unit with ID: {Id}", id);

        var unit = await _context.QuantityUnits.FindAsync(id);
        if (unit == null)
        {
            _logger.LogWarning("Quantity unit with ID {Id} not found", id);
            throw new KeyNotFoundException($"Quantity unit with id {id} not found");
        }

        return unit;
    }

    // Crea una nuova unità di misura nel DB
    public async Task<QuantityUnit> CreateQtyUnitAsync(QuantityUnit quantityUnit)
    {
        _logger.LogInformation("Creating quantity unit: {Name}", quantityUnit.Name);

        // Pulisce e valida i campi (Name e Symbol sono obbligatori)
        string name = quantityUnit.Name?.Trim() ?? "";
        string symbol = quantityUnit.Symbol?.Trim() ?? "";
        ValidateFields(name, symbol);

        // Il nome deve essere unico (la collation _ci rende il confronto case-insensitive)
        if (await _context.QuantityUnits.AnyAsync(u => u.Name == name))
        {
            _logger.LogWarning("Quantity unit already exists: {Name}", name);
            throw new ArgumentException("A quantity unit with this name already exists.");
        }

        // Crea un nuovo oggetto, così ignora un eventuale ID inviato dal client
        // e lascia che sia il DB a generarlo (AUTO_INCREMENT)
        var newUnit = new QuantityUnit { Name = name, Symbol = symbol };

        try
        {
            _context.QuantityUnits.Add(newUnit);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DB error while creating quantity unit: {Name}", name);
            throw new InvalidOperationException("Database error while creating the quantity unit.");
        }

        _logger.LogInformation("Quantity unit created: ID {Id}, Name {Name}", newUnit.IdQuantityUnit, newUnit.Name);
        return newUnit;
    }

    // Elimina un'unità di misura dal DB tramite il suo ID
    public async Task DeleteQtyUnitAsync(int id)
    {
        _logger.LogInformation("Deleting quantity unit with ID: {Id}", id);

        var unit = await _context.QuantityUnits.FindAsync(id);
        if (unit == null)
        {
            _logger.LogWarning("Quantity unit with ID {Id} not found", id);
            throw new KeyNotFoundException($"Quantity unit with id {id} not found");
        }

        // Se l'unità è usata da almeno un item non si può eliminare (FK con Restrict)
        // Controlliamo prima, così evitiamo l'eccezione del DB
        if (await _context.Items.AnyAsync(i => i.IdQuantityUnit == id))
        {
            _logger.LogWarning("Quantity unit {Id} is in use by some items, cannot delete", id);
            throw new InvalidOperationException("Cannot delete: the quantity unit is used by some items.");
        }

        try
        {
            _context.QuantityUnits.Remove(unit);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Rete di sicurezza: un item potrebbe essere stato creato tra il controllo e il salvataggio
            _logger.LogError(ex, "DB error while deleting quantity unit ID {Id}", id);
            throw new InvalidOperationException("Cannot delete: the quantity unit is used by some items.");
        }

        _logger.LogInformation("Quantity unit with ID {Id} deleted successfully", id);
    }

    // Aggiorna un'unità di misura esistente nel DB
    public async Task UpdateQtyUnitAsync(int id, QuantityUnit quantityUnit)
    {
        _logger.LogInformation("Updating quantity unit with ID: {Id}", id);

        // L'ID nell'URL deve coincidere con quello nel body
        if (id != quantityUnit.IdQuantityUnit)
        {
            _logger.LogWarning("ID mismatch: route ID {Id} does not match body ID {BodyId}", id, quantityUnit.IdQuantityUnit);
            throw new ArgumentException("The URL ID does not match the ID in the request body.");
        }

        string name = quantityUnit.Name?.Trim() ?? "";
        string symbol = quantityUnit.Symbol?.Trim() ?? "";
        ValidateFields(name, symbol);

        var unitInDb = await _context.QuantityUnits.FindAsync(id);
        if (unitInDb == null)
        {
            _logger.LogWarning("Quantity unit with ID {Id} not found", id);
            throw new KeyNotFoundException($"Quantity unit with id {id} not found");
        }

        // Il nome non deve essere già usato da un'altra unità
        if (await _context.QuantityUnits.AnyAsync(u => u.Name == name && u.IdQuantityUnit != id))
        {
            _logger.LogWarning("Update rejected: another quantity unit named '{Name}' already exists", name);
            throw new ArgumentException("A quantity unit with this name already exists.");
        }

        unitInDb.Name = name;
        unitInDb.Symbol = symbol;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DB error while updating quantity unit ID {Id}", id);
            throw new InvalidOperationException("Database error while updating the quantity unit.");
        }

        _logger.LogInformation("Quantity unit with ID {Id} updated successfully", id);
    }

    // Controlla che nome e simbolo non siano vuoti e rispettino le lunghezze del DB
    private static void ValidateFields(string name, string symbol)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be empty.");
        if (name.Length > 45)
            throw new ArgumentException("Name cannot be longer than 45 characters.");

        if (string.IsNullOrEmpty(symbol))
            throw new ArgumentException("Symbol cannot be empty.");
        // Per le emoji .NET conta più caratteri del DB, quindi questo controllo è prudente
        if (symbol.Length > 16)
            throw new ArgumentException("Symbol cannot be longer than 16 characters.");
    }
}