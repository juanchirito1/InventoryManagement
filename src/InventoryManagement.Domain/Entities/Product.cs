using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;

namespace InventoryManagement.Domain.Entities;

public class Product : BaseEntity
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int CurrentStock { get; private set; }
    public int MinStockAlert { get; set; } = 5;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    // Constructor sin parámetros para EF Core
    public Product() { }

    public Product(string sku, string name, decimal unitPrice, decimal costPrice, int initialStock, int minStockAlert, int categoryId)
    {
        SKU = sku;
        Name = name;
        UnitPrice = unitPrice;
        CostPrice = costPrice;
        MinStockAlert = minStockAlert;
        CategoryId = categoryId;

        if (initialStock > 0)
        {
            AddStock(initialStock, MovementType.InitialStock, "Stock inicial de apertura");
        }
    }

    public void AddStock(int quantity, MovementType type, string reason, string? referenceDocument = null)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad a ingresar debe ser mayor que 0.");

        CurrentStock += quantity;
        UpdatedAt = DateTime.UtcNow;

        StockMovements.Add(new StockMovement
        {
            ProductId = Id,
            MovementType = type,
            Quantity = quantity,
            StockAfterMovement = CurrentStock,
            Reason = reason,
            ReferenceDocument = referenceDocument
        });
    }

    public void ReduceStock(int quantity, MovementType type, string reason, string? referenceDocument = null)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad a retirar debe ser mayor que 0.");

        if (CurrentStock < quantity)
            throw new InsufficientStockException(Name, quantity, CurrentStock);

        CurrentStock -= quantity;
        UpdatedAt = DateTime.UtcNow;

        StockMovements.Add(new StockMovement
        {
            ProductId = Id,
            MovementType = type,
            Quantity = -quantity,
            StockAfterMovement = CurrentStock,
            Reason = reason,
            ReferenceDocument = referenceDocument
        });
    }

    public bool IsBelowMinStock() => CurrentStock <= MinStockAlert;
}
