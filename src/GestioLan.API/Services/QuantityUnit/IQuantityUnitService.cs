using GestioLan.API.Models;

namespace GestioLan.API.Services.QuantityUnits;

public interface IQuantityUnitService
{
    // Ottiene la lista di tutte le unità di misura
    Task<IEnumerable<QuantityUnit>> GetQtyUnitsAsync();

    // Ottiene la singola unità tramite il suo ID
    Task<QuantityUnit> GetQtyUnitByIdAsync(int id);

    // Crea una nuova unità di misura nel DB
    Task<QuantityUnit> CreateQtyUnitAsync(QuantityUnit quantityUnit);

    // Elimina un'unità di misura dal DB tramite il suo ID
    Task DeleteQtyUnitAsync(int id);

    // Aggiorna un'unità di misura esistente nel DB
    Task UpdateQtyUnitAsync(int id, QuantityUnit quantityUnit);
}