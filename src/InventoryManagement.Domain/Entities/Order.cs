using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;

namespace InventoryManagement.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public void AddItem(Product product, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("No se pueden agregar productos a una orden que no esté pendiente.");

        if (quantity <= 0)
            throw new DomainException("La cantidad debe ser mayor a 0.");

        var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            existingItem.SubTotal = existingItem.Quantity * existingItem.UnitPrice;
        }
        else
        {
            Items.Add(new OrderItem
            {
                OrderId = Id,
                ProductId = product.Id,
                Product = product,
                Quantity = quantity,
                UnitPrice = product.UnitPrice,
                SubTotal = quantity * product.UnitPrice
            });
        }

        RecalculateTotal();
    }

    public void Complete()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Solo se pueden completar órdenes pendientes.");

        if (!Items.Any())
            throw new DomainException("La orden debe tener al menos un ítem.");

        Status = OrderStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            throw new DomainException("La orden ya se encuentra cancelada.");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.SubTotal);
    }
}
