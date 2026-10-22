using InventoryManagement.Application.DTOs.Dashboard;
using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var totalProducts = await _context.Products.CountAsync();
        var lowStockProducts = await _context.Products
            .Where(p => p.CurrentStock <= p.MinStockAlert)
            .Include(p => p.Category)
            .ToListAsync();

        var totalOrders = await _context.Orders.CountAsync();
        var completedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Completed);
        var totalRevenue = await _context.Orders
            .Where(o => o.Status == OrderStatus.Completed)
            .SumAsync(o => o.TotalAmount);

        var recentMovements = await _context.StockMovements
            .Include(m => m.Product)
            .OrderByDescending(m => m.CreatedAt)
            .Take(10)
            .Select(m => new StockMovementDto
            {
                Id = m.Id,
                ProductId = m.ProductId,
                ProductName = m.Product != null ? m.Product.Name : string.Empty,
                MovementType = m.MovementType,
                Quantity = m.Quantity,
                StockAfterMovement = m.StockAfterMovement,
                Reason = m.Reason,
                ReferenceDocument = m.ReferenceDocument,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        return new DashboardSummaryDto
        {
            TotalProducts = totalProducts,
            LowStockProductsCount = lowStockProducts.Count,
            TotalOrders = totalOrders,
            CompletedOrders = completedOrders,
            TotalRevenue = totalRevenue,
            LowStockAlerts = lowStockProducts.Select(p => new ProductDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                Description = p.Description,
                UnitPrice = p.UnitPrice,
                CostPrice = p.CostPrice,
                CurrentStock = p.CurrentStock,
                MinStockAlert = p.MinStockAlert,
                IsLowStock = true,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? string.Empty
            }).ToList(),
            RecentMovements = recentMovements
        };
    }
}
