using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Application.DTOs.Product;

public class ProductDto
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int CurrentStock { get; set; }
    public int MinStockAlert { get; set; }
    public bool IsLowStock { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class CreateProductDto
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int InitialStock { get; set; }
    public int MinStockAlert { get; set; } = 5;
    public int CategoryId { get; set; }
}

public class AdjustStockDto
{
    public int Quantity { get; set; }
    public MovementType MovementType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceDocument { get; set; }
}

public class StockMovementDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public MovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public int StockAfterMovement { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceDocument { get; set; }
    public DateTime CreatedAt { get; set; }
}
