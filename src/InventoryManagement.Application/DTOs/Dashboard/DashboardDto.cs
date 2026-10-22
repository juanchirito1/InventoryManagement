using InventoryManagement.Application.DTOs.Product;

namespace InventoryManagement.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalProducts { get; set; }
    public int LowStockProductsCount { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<ProductDto> LowStockAlerts { get; set; } = new();
    public List<StockMovementDto> RecentMovements { get; set; } = new();
}
