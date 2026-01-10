using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs;
using Merge.Application.Exceptions;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Cart;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Interfaces.Notification;
using Merge.Application.Services.Order;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using OrderEntity = Merge.Domain.Entities.Order;

namespace Merge.Tests.Tests.Application.Services;

/// <summary>
/// Unit tests for OrderService
/// Tests cover order retrieval, creation, status updates, and cancellation
/// Uses Moq for mocking dependencies
/// </summary>
public class OrderServiceTests
{
    private readonly Mock<IRepository<OrderEntity>> _orderRepositoryMock;
    private readonly Mock<IRepository<OrderItem>> _orderItemRepositoryMock;
    private readonly Mock<ICartService> _cartServiceMock;
    private readonly Mock<IDbContext> _dbContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly Mock<MediatR.IMediator> _mediatorMock;
    private readonly IOptions<OrderSettings> _orderSettings;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IRepository<OrderEntity>>();
        _orderItemRepositoryMock = new Mock<IRepository<OrderItem>>();
        _cartServiceMock = new Mock<ICartService>();
        _dbContextMock = new Mock<IDbContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<OrderService>>();
        _mediatorMock = new Mock<MediatR.IMediator>();
        _orderSettings = Options.Create(new OrderSettings
        {
            FreeShippingThreshold = 500,
            TaxRate = 0.18m,
            DefaultShippingCost = 50
        });

        _orderService = new OrderService(
            _orderRepositoryMock.Object,
            _orderItemRepositoryMock.Object,
            _cartServiceMock.Object,
            _mediatorMock.Object,
            _dbContextMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _orderSettings);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expectedOrder = CreateTestOrder(orderId, userId);
        var expectedDto = CreateTestOrderDto(orderId, userId);

        SetupMockDbSet(new List<OrderEntity> { expectedOrder });
        _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<OrderEntity>()))
            .Returns(expectedDto);

        // Act
        var result = await _orderService.GetByIdAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        SetupMockDbSet(new List<OrderEntity>());

        // Act
        var result = await _orderService.GetByIdAsync(orderId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetOrdersByUserIdAsync Tests

    [Fact]
    public async Task GetOrdersByUserIdAsync_WithValidUserId_ShouldReturnOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = new List<OrderEntity>
        {
            CreateTestOrder(Guid.NewGuid(), userId),
            CreateTestOrder(Guid.NewGuid(), userId)
        };
        var expectedDtos = orders.Select(o => CreateTestOrderDto(o.Id, userId)).ToList();

        SetupMockDbSet(orders);
        _mapperMock.Setup(m => m.Map<IEnumerable<OrderDto>>(It.IsAny<IEnumerable<OrderEntity>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _orderService.GetOrdersByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrdersByUserIdAsync_WithNoOrders_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupMockDbSet(new List<OrderEntity>());
        _mapperMock.Setup(m => m.Map<IEnumerable<OrderDto>>(It.IsAny<IEnumerable<OrderEntity>>()))
            .Returns(new List<OrderDto>());

        // Act
        var result = await _orderService.GetOrdersByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region UpdateOrderStatusAsync Tests

    [Fact]
    public async Task UpdateOrderStatusAsync_WithValidTransition_ShouldUpdateStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = CreateTestOrder(orderId, userId);
        var product = CreateTestProduct();
        // Add an item to the order (required for status transitions)
        order.AddItem(product, 1);

        var updatedDto = CreateTestOrderDto(orderId, userId);
        updatedDto.Status = OrderStatus.Processing.ToString();

        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        SetupMockDbSet(new List<OrderEntity> { order });
        _mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<OrderEntity>()))
            .Returns(updatedDto);

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Processing);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Processing.ToString());
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithInvalidOrderId_ShouldThrowNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderEntity?)null);

        // Act
        var act = async () => await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Processing);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region CancelOrderAsync Tests

    [Fact]
    public async Task CancelOrderAsync_WithPendingOrder_ShouldCancelSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = CreateTestOrder(orderId, userId);
        var product = CreateTestProduct();
        order.AddItem(product, 2);

        var orderList = new List<OrderEntity> { order };
        SetupMockDbSetWithTracking(orderList);

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _orderRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.CancelOrderAsync(orderId);

        // Assert
        result.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_WithNonExistentOrder_ShouldReturnFalse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        SetupMockDbSet(new List<OrderEntity>());

        // Act
        var result = await _orderService.CancelOrderAsync(orderId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelOrderAsync_WithDeliveredOrder_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = CreateTestOrder(orderId, userId);
        var product = CreateTestProduct();
        order.AddItem(product, 1);
        order.Confirm();
        order.Ship();
        order.Deliver();

        SetupMockDbSetWithTracking(new List<OrderEntity> { order });

        // Act
        var act = async () => await _orderService.CancelOrderAsync(orderId);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*iptal edilemez*");
    }

    [Fact]
    public async Task CancelOrderAsync_WithShippedOrder_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = CreateTestOrder(orderId, userId);
        var product = CreateTestProduct();
        order.AddItem(product, 1);
        order.Confirm();
        order.Ship();

        SetupMockDbSetWithTracking(new List<OrderEntity> { order });

        // Act
        var act = async () => await _orderService.CancelOrderAsync(orderId);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*iptal edilemez*");
    }

    #endregion

    #region Export Tests

    [Fact]
    public async Task ExportOrdersToCsvAsync_ShouldReturnValidCsvBytes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = new List<OrderEntity>
        {
            CreateTestOrder(Guid.NewGuid(), userId)
        };
        var orderDtos = orders.Select(o => CreateTestOrderDto(o.Id, userId)).ToList();

        SetupMockDbSet(orders);
        _mapperMock.Setup(m => m.Map<List<OrderDto>>(It.IsAny<IEnumerable<OrderEntity>>()))
            .Returns(orderDtos);

        var exportDto = new OrderExportDto
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
        };

        // Act
        var result = await _orderService.ExportOrdersToCsvAsync(exportDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();

        var csvContent = System.Text.Encoding.UTF8.GetString(result);
        csvContent.Should().Contain("OrderNumber");
        csvContent.Should().Contain("TotalAmount");
    }

    [Fact]
    public async Task ExportOrdersToJsonAsync_ShouldReturnValidJsonBytes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = new List<OrderEntity>
        {
            CreateTestOrder(Guid.NewGuid(), userId)
        };
        var orderDtos = orders.Select(o => CreateTestOrderDto(o.Id, userId)).ToList();

        SetupMockDbSet(orders);
        _mapperMock.Setup(m => m.Map<List<OrderDto>>(It.IsAny<IEnumerable<OrderEntity>>()))
            .Returns(orderDtos);

        var exportDto = new OrderExportDto
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
        };

        // Act
        var result = await _orderService.ExportOrdersToJsonAsync(exportDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();

        var jsonContent = System.Text.Encoding.UTF8.GetString(result);
        jsonContent.Should().Contain("OrderNumber");
    }

    #endregion

    #region Helper Methods

    private static OrderEntity CreateTestOrder(Guid orderId, Guid userId)
    {
        var addressId = Guid.NewGuid();
        var address = Address.Create(
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

        var order = OrderEntity.Create(userId, addressId, address);

        // Use reflection to set the Id since it's set in the factory method
        var idProperty = typeof(OrderEntity).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(order, orderId);
        }
        else
        {
            // If property can't be set directly, use BaseEntity field
            var baseEntityField = typeof(BaseEntity).GetField("Id",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (baseEntityField == null)
            {
                var idField = typeof(BaseEntity).GetProperty("Id");
                // The Id is already set by factory, just return the order as is for testing
            }
        }

        return order;
    }

    private static Product CreateTestProduct()
    {
        return Product.Create(
            name: "Test Product",
            description: "Test Description",
            sku: new SKU($"TEST-{Guid.NewGuid().ToString().Substring(0, 8)}"),
            price: new Money(100),
            stockQuantity: 100,
            categoryId: Guid.NewGuid(),
            brand: "Test Brand"
        );
    }

    private static OrderDto CreateTestOrderDto(Guid orderId, Guid userId)
    {
        return new OrderDto
        {
            Id = orderId,
            UserId = userId,
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-123456",
            SubTotal = 100,
            ShippingCost = 50,
            Tax = 18,
            TotalAmount = 168,
            Status = OrderStatus.Pending.ToString(),
            PaymentStatus = PaymentStatus.Pending.ToString(),
            CreatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItemDto>(),
            Address = new Merge.Application.DTOs.User.AddressDto
            {
                Id = Guid.NewGuid(),
                AddressLine1 = "123 Test Street",
                City = "Istanbul",
                Country = "Turkey",
                PostalCode = "34000"
            }
        };
    }

    private void SetupMockDbSet(List<OrderEntity> orders)
    {
        var mockDbSet = CreateMockDbSet(orders);
        _dbContextMock.Setup(c => c.Set<OrderEntity>())
            .Returns(mockDbSet.Object);
    }

    private void SetupMockDbSetWithTracking(List<OrderEntity> orders)
    {
        var mockDbSet = CreateMockDbSet(orders, trackChanges: true);
        _dbContextMock.Setup(c => c.Set<OrderEntity>())
            .Returns(mockDbSet.Object);
    }

    private static Mock<DbSet<OrderEntity>> CreateMockDbSet(List<OrderEntity> data, bool trackChanges = false)
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<OrderEntity>>();

        mockSet.As<IQueryable<OrderEntity>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<OrderEntity>(queryable.Provider));

        mockSet.As<IQueryable<OrderEntity>>()
            .Setup(m => m.Expression)
            .Returns(queryable.Expression);

        mockSet.As<IQueryable<OrderEntity>>()
            .Setup(m => m.ElementType)
            .Returns(queryable.ElementType);

        mockSet.As<IQueryable<OrderEntity>>()
            .Setup(m => m.GetEnumerator())
            .Returns(() => queryable.GetEnumerator());

        mockSet.As<IAsyncEnumerable<OrderEntity>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<OrderEntity>(queryable.GetEnumerator()));

        return mockSet;
    }

    #endregion

    #region Test Async Helpers

    private class TestAsyncQueryProvider<TEntity> : Microsoft.EntityFrameworkCore.Query.IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(
                    name: nameof(IQueryProvider.Execute),
                    genericParameterCount: 1,
                    types: new[] { typeof(System.Linq.Expressions.Expression) })
                ?.MakeGenericMethod(expectedResultType)
                .Invoke(this, new[] { expression });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                ?.MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }

    private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }

    #endregion
}
