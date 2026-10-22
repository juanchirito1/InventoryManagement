using FluentAssertions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using Xunit;

namespace InventoryManagement.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void AddStock_WithPositiveQuantity_ShouldIncreaseCurrentStockAndAddMovement()
    {
        // Arrange
        var product = new Product("TEST-01", "Teclado Mecánico", 50.0m, 25.0m, 10, 5, 1);

        // Act
        product.AddStock(5, MovementType.Purchase, "Reposición de stock");

        // Assert
        product.CurrentStock.Should().Be(15);
        product.StockMovements.Should().HaveCount(2); // 1 inicial + 1 reposición
        product.StockMovements.Last().Quantity.Should().Be(5);
        product.StockMovements.Last().StockAfterMovement.Should().Be(15);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void AddStock_WithZeroOrNegativeQuantity_ShouldThrowDomainException(int invalidQuantity)
    {
        // Arrange
        var product = new Product("TEST-01", "Teclado Mecánico", 50.0m, 25.0m, 10, 5, 1);

        // Act
        var act = () => product.AddStock(invalidQuantity, MovementType.Purchase, "Inválido");

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("La cantidad a ingresar debe ser mayor que 0.");
    }

    [Fact]
    public void ReduceStock_WithSufficientStock_ShouldDecreaseStockAndAddMovement()
    {
        // Arrange
        var product = new Product("TEST-01", "Teclado Mecánico", 50.0m, 25.0m, 10, 5, 1);

        // Act
        product.ReduceStock(4, MovementType.Sale, "Venta #1");

        // Assert
        product.CurrentStock.Should().Be(6);
        product.StockMovements.Last().Quantity.Should().Be(-4);
        product.StockMovements.Last().StockAfterMovement.Should().Be(6);
    }

    [Fact]
    public void ReduceStock_WithInsufficientStock_ShouldThrowInsufficientStockException()
    {
        // Arrange
        var product = new Product("TEST-01", "Monitor 24", 150.0m, 100.0m, 3, 2, 1);

        // Act
        var act = () => product.ReduceStock(5, MovementType.Sale, "Venta grande");

        // Assert
        act.Should().Throw<InsufficientStockException>()
           .WithMessage("*Stock insuficiente para 'Monitor 24'. Solicitado: 5, Disponible: 3.*");
        product.CurrentStock.Should().Be(3); // El stock no se altera
    }

    [Fact]
    public void IsBelowMinStock_ShouldReturnTrue_WhenStockEqualsOrBelowThreshold()
    {
        // Arrange
        var product = new Product("TEST-01", "Mouse", 20.0m, 10.0m, 2, 5, 1);

        // Act & Assert
        product.IsBelowMinStock().Should().BeTrue();
    }
}
