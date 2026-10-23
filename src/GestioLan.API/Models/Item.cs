using System;
using System.Collections.Generic;

namespace GestioLan.API.Models;

public partial class Item
{
    public int IdItem { get; set; }

    public string ItemName { get; set; } = null!;

    public string? Description { get; set; }

    public int? IdImage { get; set; }

    public string? ImageName { get; set; }

    public int? IdCategory { get; set; }

    public int Quantity { get; set; }

    public int? IdQuantityUnit { get; set; }
    
    public QuantityUnit? QuantityUnit { get; set; }

    public string? Barcode { get; set; }
}
