using InventoryManagement.Application.DTOs.Dashboard;

namespace InventoryManagement.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}
