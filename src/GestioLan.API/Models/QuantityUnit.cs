namespace GestioLan.API.Models;

public partial class QuantityUnit
{
    public int IdQuantityUnit { get; set; }
    public string Name { get; set; } = null!;     // "Chilogrammi", "Pezzi"
    public string Symbol { get; set; } = null!;   // "kg", "🍎"
}