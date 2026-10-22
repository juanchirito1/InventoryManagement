using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(string? search = null, int? categoryId = null, bool? onlyLowStock = null)
    {
        var query = _context.Products.Include(p => p.Category).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchLower) || p.SKU.ToLower().Contains(searchLower));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (onlyLowStock.HasValue && onlyLowStock.Value)
        {
            query = query.Where(p => p.CurrentStock <= p.MinStockAlert);
        }

        return await query.Select(p => new ProductDto
        {
            Id = p.Id,
            SKU = p.SKU,
            Name = p.Name,
            Description = p.Description,
            UnitPrice = p.UnitPrice,
            CostPrice = p.CostPrice,
            CurrentStock = p.CurrentStock,
            MinStockAlert = p.MinStockAlert,
            IsLowStock = p.CurrentStock <= p.MinStockAlert,
            CategoryId = p.CategoryId,
            CategoryName = p.Category != null ? p.Category.Name : string.Empty
        }).ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var p = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (p == null) return null;

        return new ProductDto
        {
            Id = p.Id,
            SKU = p.SKU,
            Name = p.Name,
            Description = p.Description,
            UnitPrice = p.UnitPrice,
            CostPrice = p.CostPrice,
            CurrentStock = p.CurrentStock,
            MinStockAlert = p.MinStockAlert,
            IsLowStock = p.IsBelowMinStock(),
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? string.Empty
        };
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var exists = await _context.Products.AnyAsync(p => p.SKU == dto.SKU);
        if (exists)
            throw new DomainException($"Ya existe un producto con el código SKU '{dto.SKU}'.");

        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null)
            throw new DomainException($"La categoría con ID {dto.CategoryId} no existe.");

        var product = new Product(
            dto.SKU,
            dto.Name,
            dto.UnitPrice,
            dto.CostPrice,
            dto.InitialStock,
            dto.MinStockAlert,
            dto.CategoryId
        )
        {
            Description = dto.Description
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            UnitPrice = product.UnitPrice,
            CostPrice = product.CostPrice,
            CurrentStock = product.CurrentStock,
            MinStockAlert = product.MinStockAlert,
            IsLowStock = product.IsBelowMinStock(),
            CategoryId = product.CategoryId,
            CategoryName = category.Name
        };
    }

    public async Task<ProductDto> AdjustStockAsync(int id, AdjustStockDto dto)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.StockMovements)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            throw new DomainException($"El producto con ID {id} no fue encontrado.");

        if (dto.Quantity > 0)
        {
            product.AddStock(dto.Quantity, dto.MovementType, dto.Reason, dto.ReferenceDocument);
        }
        else if (dto.Quantity < 0)
        {
            product.ReduceStock(Math.Abs(dto.Quantity), dto.MovementType, dto.Reason, dto.ReferenceDocument);
        }
        else
        {
            throw new DomainException("La cantidad para ajuste debe ser distinta de cero.");
        }

        await _context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            UnitPrice = product.UnitPrice,
            CostPrice = product.CostPrice,
            CurrentStock = product.CurrentStock,
            MinStockAlert = product.MinStockAlert,
            IsLowStock = product.IsBelowMinStock(),
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty
        };
    }

    public async Task<IEnumerable<StockMovementDto>> GetMovementsAsync(int? productId = null)
    {
        var query = _context.StockMovements
            .Include(m => m.Product)
            .AsNoTracking()
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(m => m.ProductId == productId.Value);
        }

        return await query
            .OrderByDescending(m => m.CreatedAt)
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
    }
}
