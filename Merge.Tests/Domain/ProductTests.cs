using FluentAssertions;
using Merge.Domain.Entities;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Tests.Domain;

// ✅ BOLUM 13.0: Test projesi (ZORUNLU - minimum %60 coverage)
public class ProductTests
{
    [Fact]
    public void SetPrice_NegativePrice_ThrowsException()
    {
        // Arrange
        var product = Product.Create(
            name: "Test Product",
            description: "Test Description",
            sku: new SKU("TEST-001"),
            price: new Money(100),
            stockQuantity: 5,
            categoryId: Guid.NewGuid(),
            brand: "Test Brand");

        // Act & Assert
        // Money constructor negatif değer kabul etmiyor (ArgumentException fırlatır)
        // SetPrice metoduna ulaşmadan önce Money constructor'ı exception fırlatır
        Assert.Throws<ArgumentException>(() => product.SetPrice(new Money(-10)));
    }

    [Fact]
    public void ReduceStock_InsufficientStock_ThrowsException()
    {
        // Arrange
        var product = Product.Create(
            name: "Test Product",
            description: "Test Description",
            sku: new SKU("TEST-001"),
            price: new Money(100),
            stockQuantity: 5,
            categoryId: Guid.NewGuid(),
            brand: "Test Brand");

        // Act & Assert
        Assert.Throws<DomainException>(() => product.ReduceStock(10));
    }

    [Fact]
    public void ReduceStock_ValidQuantity_ReducesStock()
    {
        // Arrange
        var product = Product.Create(
            name: "Test Product",
            description: "Test Description",
            sku: new SKU("TEST-001"),
            price: new Money(100),
            stockQuantity: 10,
            categoryId: Guid.NewGuid(),
            brand: "Test Brand");

        // Act
        product.ReduceStock(3);

        // Assert
        product.StockQuantity.Should().Be(7);
    }
}

