---
description: Generate comprehensive unit tests for current file
allowed-tools:
  - Read
  - Write
  - Bash(dotnet test)
---

# Generate Tests

Generate comprehensive unit tests for the currently focused file.

## Test Framework
- **xUnit** for test framework
- **FluentAssertions** for assertions
- **Moq** for mocking

## Steps

1. **Read the source file** to understand:
   - Class type (Entity, Command Handler, Query Handler, Service, Controller)
   - Dependencies (constructor parameters)
   - Public methods to test
   - Business logic branches

2. **Create test file** at:
   - Source: `Merge.Application/Product/Commands/CreateProduct/CreateProductCommandHandler.cs`
   - Test: `Merge.Tests/Unit/Application/Product/Commands/CreateProductCommandHandlerTests.cs`

3. **Generate tests** covering:
   - Happy path scenarios
   - Edge cases
   - Error conditions
   - All public methods

## Test Patterns

### For Command Handlers
```csharp
public class CreateProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<Product>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateProductCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            // ... other mocks
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProduct()
    {
        // Arrange
        var command = new CreateProductCommand(...);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidPrice_ShouldThrow()
    {
        // Arrange
        var command = new CreateProductCommand(Price: -10);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
```

### For Domain Entities
```csharp
public class ProductTests
{
    [Fact]
    public void Create_ValidParameters_ShouldCreateProduct()
    {
        // Arrange & Act
        var product = Product.Create("Name", "Desc", sku, price, 100, categoryId);

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be("Name");
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreatedEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_InvalidName_ShouldThrow(string? name)
    {
        // Act & Assert
        var act = () => Product.Create(name!, ...);
        act.Should().Throw<ArgumentException>();
    }
}
```

## Naming Convention

```
[MethodName]_[Scenario]_[ExpectedResult]

Examples:
- Handle_ValidCommand_ShouldCreateProduct
- Create_InvalidPrice_ShouldThrowArgumentException
- ReduceStock_InsufficientStock_ShouldThrowDomainException
```

## After Generation

Run tests to verify: `dotnet test --filter "FullyQualifiedName~[TestClassName]"`
