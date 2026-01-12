using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// Cart Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Cart : BaseAggregateRoot
{
    private readonly List<CartItem> _cartItems = new();

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
    public void AddItem(CartItem item)
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

        if (existingItem != null)
        {
            // Update quantity instead of adding duplicate
            existingItem.UpdateQuantity(existingItem.Quantity + item.Quantity);
        }
        else
        {
            _cartItems.Add(item);
        }

        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CartItemAddedEvent yayınla
        AddDomainEvent(new CartItemAddedEvent(Id, item.ProductId, item.Quantity));
    }

    // ✅ BOLUM 1.1: Domain Logic - Remove item from cart
    public void RemoveItem(Guid cartItemId)
    {
        var item = _cartItems.FirstOrDefault(ci => ci.Id == cartItemId && !ci.IsDeleted);
        if (item == null)
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

    // ✅ BOLUM 1.1: Domain Logic - Calculate total amount
    public decimal CalculateTotalAmount()
    {
        return _cartItems
            .Where(item => !item.IsDeleted)
            .Sum(item => item.Quantity * item.Price);
    }

    // ✅ BOLUM 1.1: Domain Logic - Get item count
    public int GetItemCount()
    {
        return _cartItems.Count(item => !item.IsDeleted);
    }
}

