using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;

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
public class Cart : BaseEntity, IAggregateRoot
{
    private readonly List<CartItem> _cartItems = [];

    public Guid UserId { get; private set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;
    
    public IReadOnlyCollection<CartItem> CartItems => _cartItems.AsReadOnly();
    
    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    // BaseEntity'deki protected RemoveDomainEvent yerine public RemoveDomainEvent kullanılabilir
    // Service layer'dan event kaldırılabilmesi için public yapıldı
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected RemoveDomainEvent'i çağır
        base.RemoveDomainEvent(domainEvent);
    }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private Cart() { }

    public static Cart Create(Guid userId, User user)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNull(user, nameof(user));

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            User = user,
            CreatedAt = DateTime.UtcNow
        };

        cart.AddDomainEvent(new CartCreatedEvent(cart.Id, userId));

        return cart;
    }

    public void AddItem(CartItem item, int? maxQuantity = null)
    {
        Guard.AgainstNull(item, nameof(item));

        if (item.CartId != Id)
            throw new DomainException("CartItem bu sepete ait değil");

        // Check if item already exists (same product and variant)
        var existingItem = _cartItems.FirstOrDefault(ci => 
            ci.ProductId == item.ProductId && 
            ci.ProductVariantId == item.ProductVariantId &&
            !ci.IsDeleted);

        if (existingItem is not null)
        {
            // Update quantity instead of adding duplicate
            var oldQuantity = existingItem.Quantity;
            var newQuantity = existingItem.Quantity + item.Quantity;
            existingItem.UpdateQuantity(newQuantity, maxQuantity);
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new CartItemQuantityUpdatedEvent(Id, existingItem.Id, item.ProductId, oldQuantity, newQuantity));
        }
        else
        {
            // Validate quantity if maxQuantity is provided
            if (maxQuantity.HasValue && item.Quantity > maxQuantity.Value)
            {
                throw new DomainException($"Miktar maksimum {maxQuantity.Value} olabilir.");
            }
            _cartItems.Add(item);
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new CartItemAddedEvent(Id, item.ProductId, item.Quantity));
        }
    }

    public void UpdateItemQuantity(Guid cartItemId, int newQuantity, int? maxQuantity = null)
    {
        Guard.AgainstDefault(cartItemId, nameof(cartItemId));
        Guard.AgainstNegativeOrZero(newQuantity, nameof(newQuantity));

        var item = _cartItems.FirstOrDefault(ci => ci.Id == cartItemId && !ci.IsDeleted);
        
        if (item is null)
            throw new DomainException($"Sepet öğesi bulunamadı: {cartItemId}");

        var oldQuantity = item.Quantity;
        
        item.UpdateQuantity(newQuantity, maxQuantity);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CartItemQuantityUpdatedEvent(Id, cartItemId, item.ProductId, oldQuantity, newQuantity));
    }

    public void RemoveItem(Guid cartItemId)
    {
        var item = _cartItems.FirstOrDefault(ci => ci.Id == cartItemId && !ci.IsDeleted);
        
        if (item is null)
            throw new DomainException($"Sepet öğesi bulunamadı: {cartItemId}");

        item.MarkAsDeleted();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CartItemRemovedEvent(Id, item.ProductId));
    }

    public void Clear()
    {
        foreach (var item in _cartItems.Where(ci => !ci.IsDeleted))
        {
            item.MarkAsDeleted();
        }

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CartClearedEvent(Id, UserId));
    }

    public decimal CalculateTotalAmount()
    {
        return _cartItems
            .Where(item => !item.IsDeleted)
            .Sum(item => item.Quantity * item.Price);
    }

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

    public int GetItemCount()
    {
        return _cartItems.Count(item => !item.IsDeleted);
    }
}

