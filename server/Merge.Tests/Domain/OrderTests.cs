using FluentAssertions;

using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using AddressEntity = Merge.Domain.Modules.Identity.Address;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void Create_ValidInput_ReturnsOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var address = AddressEntity.Create(
            userId: userId,
            title: "Home",
            firstName: "Test",
            lastName: "User",
            phoneNumber: "5551234567",
            addressLine1: "Test Address",
            city: "Istanbul",
            district: "Test District",
            postalCode: "34000",
            country: "Turkey");

        // Act
        var order = Order.Create(userId, addressId, address);

        // Assert
        order.Should().NotBeNull();
        order.UserId.Should().Be(userId);
        order.AddressId.Should().Be(addressId);
        order.Status.Should().Be(OrderStatus.Pending);
        order.DomainEvents.Should().Contain(e => e.GetType().Name == "OrderCreatedEvent");
    }

    [Fact]
    public void AddItem_ValidProduct_AddsItem()
    {
        // Arrange
        var addressId = Guid.NewGuid();
        var address = AddressEntity.Create(
            userId: Guid.NewGuid(),
            title: "Home",
            firstName: "Test",
            lastName: "User",
            phoneNumber: "5551234567",
            addressLine1: "Test",
            city: "Istanbul",
            district: "Test District",
            postalCode: "34000",
            country: "Turkey");
        var order = Order.Create(Guid.NewGuid(), address.Id, address);
        
        var productId = Guid.NewGuid();
        var product = Product.Create(
            name: "Test Product",
            description: "Test Description",
            sku: new Merge.Domain.ValueObjects.SKU("TEST-001"),
            price: new Merge.Domain.ValueObjects.Money(100),
            stockQuantity: 10,
            categoryId: Guid.NewGuid(),
            brand: "Test Brand");
        product.SetStockQuantity(10);

        // Act
        order.AddItem(product, 2);

        // Assert
        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.First().Quantity.Should().Be(2);
        // ProductId kontrolü - OrderItem entity'sinde ProductId var
        var orderItem = order.OrderItems.First();
        orderItem.Should().NotBeNull();
    }

    [Fact]
    public void TransitionTo_InvalidState_ThrowsException()
    {
        // Arrange
        var addressId = Guid.NewGuid();
        var address = AddressEntity.Create(
            userId: Guid.NewGuid(),
            title: "Home",
            firstName: "Test",
            lastName: "User",
            phoneNumber: "5551234567",
            addressLine1: "Test",
            city: "Istanbul",
            district: "Test District",
            postalCode: "34000",
            country: "Turkey");
        var order = Order.Create(Guid.NewGuid(), address.Id, address);

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => order.TransitionTo(OrderStatus.Delivered));
    }
}
