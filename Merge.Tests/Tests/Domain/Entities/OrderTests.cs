using FluentAssertions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Tests.Tests.Domain.Entities;

/// <summary>
/// Unit tests for Order aggregate root entity
/// Tests cover factory method, domain logic, state machine pattern, and invariant validations
/// </summary>
public class OrderTests
{
    #region Factory Method Tests

    [Fact]
    public void Create_WithValidInput_ShouldReturnOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var address = CreateTestAddress(addressId, userId);

        // Act
        var order = Order.Create(userId, addressId, address);

        // Assert
        order.Should().NotBeNull();
        order.Id.Should().NotBeEmpty();
        order.UserId.Should().Be(userId);
        order.AddressId.Should().Be(addressId);
        order.OrderNumber.Should().StartWith("ORD-");
        order.Status.Should().Be(OrderStatus.Pending);
        order.PaymentStatus.Should().Be(PaymentStatus.Pending);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var address = CreateTestAddress(addressId, userId);

        // Act
        var order = Order.Create(userId, addressId, address);

        // Assert
        order.DomainEvents.Should().HaveCount(1);
        order.DomainEvents.First().GetType().Name.Should().Be("OrderCreatedEvent");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrowException()
    {
        // Arrange
        var addressId = Guid.NewGuid();
        var address = CreateTestAddress(addressId, Guid.NewGuid());

        // Act
        var act = () => Order.Create(Guid.Empty, addressId, address);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyAddressId_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var address = CreateTestAddress(Guid.NewGuid(), userId);

        // Act
        var act = () => Order.Create(userId, Guid.Empty, address);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region AddItem Tests

    [Fact]
    public void AddItem_WithValidProduct_ShouldAddItemToOrder()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);

        // Act
        order.AddItem(product, 2);

        // Assert
        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.First().Quantity.Should().Be(2);
        order.SubTotal.Should().Be(200);
    }

    [Fact]
    public void AddItem_WithMultipleProducts_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        var order = CreateTestOrder();
        var product1 = CreateTestProduct(stockQuantity: 10, price: 100);
        var product2 = CreateTestProduct(stockQuantity: 5, price: 50);

        // Act
        order.AddItem(product1, 2);
        order.AddItem(product2, 3);

        // Assert
        order.OrderItems.Should().HaveCount(2);
        order.SubTotal.Should().Be(350); // (100*2) + (50*3) = 200 + 150 = 350
    }

    [Fact]
    public void AddItem_WhenOrderNotPending_ShouldThrowDomainException()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);
        order.AddItem(product, 1);
        order.Confirm(); // Change status to Processing

        // Act
        var act = () => order.AddItem(product, 1);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Bekleyen olmayan*");
    }

    [Fact]
    public void AddItem_WithInsufficientStock_ShouldThrowDomainException()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 5, price: 100);

        // Act
        var act = () => order.AddItem(product, 10);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Yetersiz stok*");
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ShouldThrowException()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);

        // Act
        var act = () => order.AddItem(product, 0);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItem_WithDiscountedProduct_ShouldUseDiscountPrice()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100, discountPrice: 80);

        // Act
        order.AddItem(product, 2);

        // Assert
        order.SubTotal.Should().Be(160); // 80 * 2
    }

    #endregion

    #region RemoveItem Tests

    [Fact]
    public void RemoveItem_WithValidItemId_ShouldRemoveItem()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);
        order.AddItem(product, 2);
        var itemId = order.OrderItems.First().Id;

        // Act
        order.RemoveItem(itemId);

        // Assert
        order.OrderItems.Should().BeEmpty();
        order.SubTotal.Should().Be(0);
    }

    [Fact]
    public void RemoveItem_WhenOrderNotPending_ShouldThrowDomainException()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);
        order.AddItem(product, 1);
        var itemId = order.OrderItems.First().Id;
        order.Confirm();

        // Act
        var act = () => order.RemoveItem(itemId);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Bekleyen olmayan*");
    }

    [Fact]
    public void RemoveItem_WithInvalidItemId_ShouldThrowDomainException()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);
        order.AddItem(product, 1);

        // Act
        var act = () => order.RemoveItem(Guid.NewGuid());

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*bulunamadi*");
    }

    #endregion

    #region State Machine Tests

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Processing, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.OnHold, true)]
    [InlineData(OrderStatus.Processing, OrderStatus.Shipped, true)]
    [InlineData(OrderStatus.Processing, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered, true)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Refunded, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered, false)]
    [InlineData(OrderStatus.Pending, OrderStatus.Shipped, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Processing, false)]
    public void TransitionTo_ShouldRespectStateMachineRules(OrderStatus from, OrderStatus to, bool shouldSucceed)
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);
        order.AddItem(product, 1);

        // Navigate to the 'from' state
        NavigateToState(order, from);

        // Act
        var act = () => order.TransitionTo(to);

        // Assert
        if (shouldSucceed)
        {
            act.Should().NotThrow();
            order.Status.Should().Be(to);
        }
        else
        {
            act.Should().Throw<InvalidStateTransitionException>();
        }
    }

    [Fact]
    public void Confirm_ShouldTransitionToProcessing()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);
        order.AddItem(product, 1);

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void Ship_ShouldSetShippedDate()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);
        order.AddItem(product, 1);
        order.Confirm();

        // Act
        order.Ship();

        // Assert
        order.Status.Should().Be(OrderStatus.Shipped);
        order.ShippedDate.Should().NotBeNull();
        order.ShippedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Ship_ShouldRaiseDomainEvent()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);
        order.AddItem(product, 1);
        order.Confirm();

        // Act
        order.Ship();

        // Assert
        order.DomainEvents.Should().Contain(e => e.GetType().Name == "OrderShippedEvent");
    }

    [Fact]
    public void Deliver_ShouldSetDeliveredDate()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);
        order.AddItem(product, 1);
        order.Confirm();
        order.Ship();

        // Act
        order.Deliver();

        // Assert
        order.Status.Should().Be(OrderStatus.Delivered);
        order.DeliveredDate.Should().NotBeNull();
        order.DeliveredDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Cancel_WhenShipped_ShouldThrowDomainException()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);
        order.AddItem(product, 1);
        order.Confirm();
        order.Ship();

        // Act
        var act = () => order.Cancel();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*iptal edilemez*");
    }

    [Fact]
    public void Cancel_ShouldRaiseDomainEvent()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);
        order.AddItem(product, 1);

        // Act
        order.Cancel("Customer request");

        // Assert
        order.DomainEvents.Should().Contain(e => e.GetType().Name == "OrderCancelledEvent");
    }

    #endregion

    #region Shipping and Tax Tests

    [Fact]
    public void SetShippingCost_ShouldUpdateTotalAmount()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);
        order.AddItem(product, 1);
        var shippingCost = new Money(20);

        // Act
        order.SetShippingCost(shippingCost);

        // Assert
        order.ShippingCost.Should().Be(20);
        order.TotalAmount.Should().Be(120); // 100 + 20
    }

    [Fact]
    public void SetTax_ShouldUpdateTotalAmount()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);
        order.AddItem(product, 1);
        var tax = new Money(18);

        // Act
        order.SetTax(tax);

        // Assert
        order.Tax.Should().Be(18);
        order.TotalAmount.Should().Be(118); // 100 + 18
    }

    #endregion

    #region Payment Tests

    [Fact]
    public void SetPaymentMethod_ShouldUpdatePaymentMethod()
    {
        // Arrange
        var order = CreateTestOrder();

        // Act
        order.SetPaymentMethod("CreditCard");

        // Assert
        order.PaymentMethod.Should().Be("CreditCard");
    }

    [Fact]
    public void SetPaymentStatus_ShouldUpdatePaymentStatus()
    {
        // Arrange
        var order = CreateTestOrder();

        // Act
        order.SetPaymentStatus(PaymentStatus.Completed);

        // Assert
        order.PaymentStatus.Should().Be(PaymentStatus.Completed);
    }

    #endregion

    #region Value Object Tests

    [Fact]
    public void SubTotalMoney_ShouldReturnCorrectMoneyValueObject()
    {
        // Arrange
        var order = CreateTestOrder();
        var product = CreateTestProduct(stockQuantity: 10, price: 100);
        order.AddItem(product, 2);

        // Act
        var subTotalMoney = order.SubTotalMoney;

        // Assert
        subTotalMoney.Should().NotBeNull();
        subTotalMoney.Amount.Should().Be(200);
        subTotalMoney.Currency.Should().Be("TRY");
    }

    #endregion

    #region Helper Methods

    private static Address CreateTestAddress(Guid addressId, Guid userId)
    {
        return Address.Create(
            userId: userId,
            title: "Home",
            firstName: "John",
            lastName: "Doe",
            phoneNumber: "+905551234567",
            addressLine1: "123 Test Street",
            city: "Istanbul",
            district: "Kadikoy",
            postalCode: "34000",
            country: "Turkey"
        );
    }

    private static Order CreateTestOrder()
    {
        var userId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var address = CreateTestAddress(addressId, userId);
        return Order.Create(userId, addressId, address);
    }

    private static Product CreateTestProduct(int stockQuantity = 10, decimal price = 100, decimal? discountPrice = null)
    {
        var product = Product.Create(
            name: "Test Product",
            description: "Test Description",
            sku: new SKU($"TEST-{Guid.NewGuid().ToString().Substring(0, 8)}"),
            price: new Money(price),
            stockQuantity: stockQuantity,
            categoryId: Guid.NewGuid(),
            brand: "Test Brand"
        );

        if (discountPrice.HasValue)
        {
            product.SetDiscountPrice(new Money(discountPrice.Value));
        }

        return product;
    }

    private static void NavigateToState(Order order, OrderStatus targetState)
    {
        switch (targetState)
        {
            case OrderStatus.Pending:
                break;
            case OrderStatus.Processing:
                order.Confirm();
                break;
            case OrderStatus.Shipped:
                order.Confirm();
                order.Ship();
                break;
            case OrderStatus.Delivered:
                order.Confirm();
                order.Ship();
                order.Deliver();
                break;
            case OrderStatus.Cancelled:
                order.Cancel();
                break;
            case OrderStatus.Refunded:
                order.Confirm();
                order.Ship();
                order.Deliver();
                order.Refund();
                break;
            case OrderStatus.OnHold:
                order.PutOnHold();
                break;
        }
    }

    #endregion
}
