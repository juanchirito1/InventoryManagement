using FluentAssertions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;
using Xunit;

namespace InventoryManagement.UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void AddItem_ShouldCalculateSubTotalAndRecalculateTotalAmount()
    {
        // Arrange
        var order = new Order { CustomerId = 1, OrderNumber = "ORD-TEST-001" };
        var product1 = new Product("SKU-1", "Producto 1", 100.0m, 50.0m, 10, 2, 1) { Id = 1 };
        var product2 = new Product("SKU-2", "Producto 2", 50.0m, 20.0m, 10, 2, 1) { Id = 2 };

        // Act
        order.AddItem(product1, 2); // 200.00
        order.AddItem(product2, 3); // 150.00

        // Assert
        order.Items.Should().HaveCount(2);
        order.TotalAmount.Should().Be(350.0m);
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void AddItem_WhenProductAlreadyInOrder_ShouldAccumulateQuantity()
    {
        // Arrange
        var order = new Order { CustomerId = 1, OrderNumber = "ORD-TEST-002" };
        var product = new Product("SKU-1", "Producto 1", 100.0m, 50.0m, 10, 2, 1) { Id = 1 };

        // Act
        order.AddItem(product, 2);
        order.AddItem(product, 3);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items.First().Quantity.Should().Be(5);
        order.TotalAmount.Should().Be(500.0m);
    }

    [Fact]
    public void Complete_WhenOrderHasItems_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var order = new Order { CustomerId = 1, OrderNumber = "ORD-TEST-003" };
        var product = new Product("SKU-1", "Producto 1", 50.0m, 20.0m, 10, 2, 1) { Id = 1 };
        order.AddItem(product, 1);

        // Act
        order.Complete();

        // Assert
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void Complete_WhenOrderHasNoItems_ShouldThrowDomainException()
    {
        // Arrange
        var order = new Order { CustomerId = 1, OrderNumber = "ORD-TEST-004" };

        // Act
        var act = () => order.Complete();

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("La orden debe tener al menos un ítem.");
    }

    [Fact]
    public void Cancel_WhenOrderIsPendingOrCompleted_ShouldSetStatusToCancelled()
    {
        // Arrange
        var order = new Order { CustomerId = 1, OrderNumber = "ORD-TEST-005" };

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenOrderAlreadyCancelled_ShouldThrowDomainException()
    {
        // Arrange
        var order = new Order { CustomerId = 1, OrderNumber = "ORD-TEST-006", Status = OrderStatus.Cancelled };

        // Act
        var act = () => order.Cancel();

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("La orden ya se encuentra cancelada.");
    }
}
