using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Domain.Entities;

public class StockMovement : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public MovementType MovementType { get; set; }
    public int Quantity { get; set; } // Positivo para entrada, negativo para salida
    public int StockAfterMovement { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceDocument { get; set; } // Por ej: "ORD-2026-001" o "FAC-984"
}
