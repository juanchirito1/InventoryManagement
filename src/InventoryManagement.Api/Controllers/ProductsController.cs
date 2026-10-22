using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] bool? onlyLowStock = null)
    {
        var products = await _productService.GetAllAsync(search, categoryId, onlyLowStock);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null) return NotFound(new { message = $"Producto con ID {id} no encontrado." });
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        var created = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id}/adjust-stock")]
    public async Task<ActionResult<ProductDto>> AdjustStock(int id, [FromBody] AdjustStockDto dto)
    {
        var updated = await _productService.AdjustStockAsync(id, dto);
        return Ok(updated);
    }

    [HttpGet("movements")]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetMovements([FromQuery] int? productId = null)
    {
        var movements = await _productService.GetMovementsAsync(productId);
        return Ok(movements);
    }
}
