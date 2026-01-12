using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// Cart Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Cart : BaseAggregateRoot
{
    // ✅ BOLUM 7.1.9: Collection Expressions (C# 12) - List yerine collection expression
    private readonly List<CartItem> _cartItems = [];

    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;
    
    // ✅ BOLUM 1.4: Aggregate Root Pattern - CartItem'lara sadece Cart üzerinden erişim
    public IReadOnlyCollection<CartItem> CartItems => _cartItems.AsReadOnly();
    
    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Cart() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Cart Create(Guid userId)
    {
        Guard.AgainstDefault(userId, nameof(userId));

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - CartCreatedEvent yayınla
        cart.AddDomainEvent(new CartCreatedEvent(cart.Id, userId));

        return cart;
    }

    // ✅ BOLUM 1.1: Domain Logic - Add item to cart
    public void AddItem(CartItem item, int? maxQuantity = null)
    {
        Guard.AgainstNull(item, nameof(item));

        // ✅ BOLUM 1.6: Invariant Validation - Item must belong to this cart
        if (item.CartId != Id)
            throw new DomainException("CartItem bu sepete ait değil");

        // Check if item already exists (same product and variant)
        var existingItem = _cartItems.FirstOrDefault(ci => 
            ci.ProductId == item.ProductId && 
            ci.ProductVariantId == item.ProductVariantId &&
            !ci.IsDeleted);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (existingItem is not null)
        {
            // Update quantity instead of adding duplicate
            var newQuantity = existingItem.Quantity + item.Quantity;
            existingItem.UpdateQuantity(newQuantity, maxQuantity);
        }
        else
        {
            // Validate quantity if maxQuantity is provided
            if (maxQuantity.HasValue && item.Quantity > maxQuantity.Value)
            {
                throw new DomainException($"Miktar maksimum {maxQuantity.Value} olabilir.");
            }
            _cartItems.Add(item);
        }

        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CartItemAddedEvent yayınla
        AddDomainEvent(new CartItemAddedEvent(Id, item.ProductId, item.Quantity));
    }

    // ✅ BOLUM 1.1: Domain Logic - Remove item from cart
    // ✅ BOLUM 7.1.6: Pattern Matching - Switch expression kullanımı (modern C#)
    public void RemoveItem(Guid cartItemId)
    {
        var item = _cartItems.FirstOrDefault(ci => ci.Id == cartItemId && !ci.IsDeleted);
        
        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (item is null)
            throw new DomainException($"Sepet öğesi bulunamadı: {cartItemId}");

        item.MarkAsDeleted();
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CartItemRemovedEvent yayınla
        AddDomainEvent(new CartItemRemovedEvent(Id, item.ProductId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Clear cart
    public void Clear()
    {
        foreach (var item in _cartItems.Where(ci => !ci.IsDeleted))
        {
            item.MarkAsDeleted();
        }

        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CartClearedEvent yayınla
        AddDomainEvent(new CartClearedEvent(Id, UserId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Calculate total amount (returns decimal for backward compatibility)
    public decimal CalculateTotalAmount()
    {
        return _cartItems
            .Where(item => !item.IsDeleted)
            .Sum(item => item.Quantity * item.Price);
    }

    // ✅ BOLUM 1.3: Value Objects - Calculate total amount as Money Value Object using Money.Add
    public Money CalculateTotalAmountMoney(string currency = "TRY")
    {
        var total = Money.Zero(currency);
        
        foreach (var item in _cartItems.Where(i => !i.IsDeleted))
        {
            var itemTotal = new Money(item.Quantity * item.Price, currency);
            total = total.Add(itemTotal);
        }
        
        return total;
    }

    // ✅ BOLUM 1.1: Domain Logic - Get item count
    public int GetItemCount()
    {
        return _cartItems.Count(item => !item.IsDeleted);
    }
}

