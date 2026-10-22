using InventoryManagement.Application.DTOs.Product;

namespace InventoryManagement.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync(string? search = null, int? categoryId = null, bool? onlyLowStock = null);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto> AdjustStockAsync(int id, AdjustStockDto dto);
    Task<IEnumerable<StockMovementDto>> GetMovementsAsync(int? productId = null);
}
