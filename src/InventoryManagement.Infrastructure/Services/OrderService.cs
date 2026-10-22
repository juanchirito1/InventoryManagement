using InventoryManagement.Application.DTOs.Order;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .Select(o => MapToDto(o))
            .ToListAsync();
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null ? null : MapToDto(order);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            throw new DomainException("La orden debe incluir al menos un producto.");

        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer == null)
            throw new DomainException($"El cliente con ID {dto.CustomerId} no existe.");

        // Ejecución atómica mediante transacción
        using var transaction = await _context.Database.BeginTransactionAsync();

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
        var order = new Order
        {
            CustomerId = customer.Id,
            Customer = customer,
            OrderNumber = orderNumber,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };

        foreach (var itemDto in dto.Items)
        {
            var product = await _context.Products
                .Include(p => p.StockMovements)
                .FirstOrDefaultAsync(p => p.Id == itemDto.ProductId);

            if (product == null)
                throw new DomainException($"El producto con ID {itemDto.ProductId} no fue encontrado.");

            // Descuenta stock y genera el movimiento de inventario automáticamente
            product.ReduceStock(itemDto.Quantity, MovementType.Sale, $"Venta para la orden {orderNumber}", orderNumber);

            order.AddItem(product, itemDto.Quantity);
        }

        order.Complete();

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToDto(order);
    }

    public async Task<OrderDto> CancelOrderAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            throw new DomainException($"La orden con ID {id} no existe.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        // Reponer stock
        foreach (var item in order.Items)
        {
            var product = await _context.Products
                .Include(p => p.StockMovements)
                .FirstOrDefaultAsync(p => p.Id == item.ProductId);

            if (product != null)
            {
                product.AddStock(item.Quantity, MovementType.Return, $"Cancelación de orden {order.OrderNumber}", order.OrderNumber);
            }
        }

        order.Cancel();
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToDto(order);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.FullName ?? string.Empty,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? string.Empty,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                SubTotal = i.SubTotal
            }).ToList()
        };
    }
}
