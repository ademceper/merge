using FluentAssertions;
using Merge.Domain.ValueObjects;

namespace Merge.Tests.Tests.Domain.ValueObjects;

/// <summary>
/// Unit tests for Money value object
/// Tests cover creation, arithmetic operations, currency validation, and immutability
/// </summary>
public class MoneyTests
{
    #region Creation Tests

    [Fact]
    public void Create_WithValidAmount_ShouldCreateMoney()
    {
        // Arrange & Act
        var money = new Money(100);

        // Assert
        money.Amount.Should().Be(100);
        money.Currency.Should().Be("TRY");
    }

    [Fact]
    public void Create_WithValidAmountAndCurrency_ShouldCreateMoney()
    {
        // Arrange & Act
        var money = new Money(100, "USD");

        // Assert
        money.Amount.Should().Be(100);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithLowercaseCurrency_ShouldNormalizeTtoUppercase()
    {
        // Arrange & Act
        var money = new Money(100, "eur");

        // Assert
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldCreateMoney()
    {
        // Arrange & Act
        var money = new Money(0);

        // Assert
        money.Amount.Should().Be(0);
    }

    [Fact]
    public void Create_WithDecimalAmount_ShouldPreservePrecision()
    {
        // Arrange & Act
        var money = new Money(99.99m);

        // Assert
        money.Amount.Should().Be(99.99m);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => new Money(-100);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void Create_WithInvalidCurrencyLength_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => new Money(100, "US");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*3-letter*");
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => new Money(100, "");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceCurrency_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => new Money(100, "   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Zero Factory Method Tests

    [Fact]
    public void Zero_ShouldReturnMoneyWithZeroAmount()
    {
        // Arrange & Act
        var money = Money.Zero();

        // Assert
        money.Amount.Should().Be(0);
        money.Currency.Should().Be("TRY");
    }

    [Fact]
    public void Zero_WithCurrency_ShouldReturnZeroMoneyWithSpecifiedCurrency()
    {
        // Arrange & Act
        var money = Money.Zero("USD");

        // Assert
        money.Amount.Should().Be(0);
        money.Currency.Should().Be("USD");
    }

    #endregion

    #region Add Operation Tests

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSum()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(50);

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150);
        result.Currency.Should().Be("TRY");
    }

    [Fact]
    public void Add_ShouldBeImmutable()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(50);

        // Act
        var result = money1.Add(money2);

        // Assert
        money1.Amount.Should().Be(100);
        money2.Amount.Should().Be(50);
        result.Amount.Should().Be(150);
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100, "TRY");
        var money2 = new Money(50, "USD");

        // Act
        var act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Add_WithZero_ShouldReturnSameAmount()
    {
        // Arrange
        var money = new Money(100);
        var zero = Money.Zero();

        // Act
        var result = money.Add(zero);

        // Assert
        result.Amount.Should().Be(100);
    }

    #endregion

    #region Subtract Operation Tests

    [Fact]
    public void Subtract_WithSameCurrency_ShouldReturnDifference()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(30);

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70);
        result.Currency.Should().Be("TRY");
    }

    [Fact]
    public void Subtract_ShouldBeImmutable()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(30);

        // Act
        var result = money1.Subtract(money2);

        // Assert
        money1.Amount.Should().Be(100);
        money2.Amount.Should().Be(30);
        result.Amount.Should().Be(70);
    }

    [Fact]
    public void Subtract_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100, "TRY");
        var money2 = new Money(30, "EUR");

        // Act
        var act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Subtract_ResultingInNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var money1 = new Money(30);
        var money2 = new Money(100);

        // Act
        var act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void Subtract_SameAmount_ShouldReturnZero()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(100);

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(0);
    }

    #endregion

    #region Equality Tests (Record Behavior)

    [Fact]
    public void Equals_WithSameAmountAndCurrency_ShouldReturnTrue()
    {
        // Arrange
        var money1 = new Money(100, "TRY");
        var money2 = new Money(100, "TRY");

        // Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentAmount_ShouldReturnFalse()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(200);

        // Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCurrency_ShouldReturnFalse()
    {
        // Arrange
        var money1 = new Money(100, "TRY");
        var money2 = new Money(100, "USD");

        // Assert
        money1.Should().NotBe(money2);
    }

    [Fact]
    public void GetHashCode_ForEqualMoney_ShouldReturnSameHash()
    {
        // Arrange
        var money1 = new Money(100, "TRY");
        var money2 = new Money(100, "TRY");

        // Assert
        money1.GetHashCode().Should().Be(money2.GetHashCode());
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void Create_WithLargeAmount_ShouldHandleCorrectly()
    {
        // Arrange & Act
        var money = new Money(decimal.MaxValue / 2);

        // Assert
        money.Amount.Should().Be(decimal.MaxValue / 2);
    }

    [Fact]
    public void Create_WithVerySmallDecimal_ShouldPreservePrecision()
    {
        // Arrange & Act
        var money = new Money(0.01m);

        // Assert
        money.Amount.Should().Be(0.01m);
    }

    [Fact]
    public void Add_MultipleOperations_ShouldChainCorrectly()
    {
        // Arrange
        var money1 = new Money(100);
        var money2 = new Money(50);
        var money3 = new Money(25);

        // Act
        var result = money1.Add(money2).Add(money3);

        // Assert
        result.Amount.Should().Be(175);
    }

    #endregion
}
