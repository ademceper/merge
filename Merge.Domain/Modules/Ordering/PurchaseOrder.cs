using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Payment;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// PurchaseOrder Aggregate Root - Rich Domain Model implementation
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (ZORUNLU - String Status YASAK)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.6: Invariant Validation (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PurchaseOrder : BaseEntity, IAggregateRoot
{
    private readonly List<PurchaseOrderItem> _items = new();

    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid OrganizationId { get; private set; }
    public Guid? B2BUserId { get; private set; } // User who created the PO
    public string PONumber { get; private set; } = string.Empty; // Auto-generated: PO-XXXXXX
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public PurchaseOrderStatus Status { get; private set; } = PurchaseOrderStatus.Draft;
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - EF Core compatibility için decimal backing fields
    private decimal _subTotal;
    private decimal _tax;
    private decimal _totalAmount;
    
    // Database columns (EF Core mapping)
    public decimal SubTotal 
    { 
        get => _subTotal; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(SubTotal));
            _subTotal = value;
        }
    }
    
    public decimal Tax 
    { 
        get => _tax; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(Tax));
            _tax = value;
        }
    }
    
    public decimal TotalAmount 
    { 
        get => _totalAmount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(TotalAmount));
            _totalAmount = value;
        }
    }
    
    // ✅ BOLUM 1.3: Value Object properties (computed from decimal)
    [NotMapped]
    public Money SubTotalMoney => new Money(_subTotal);
    
    [NotMapped]
    public Money TaxMoney => new Money(_tax);
    
    [NotMapped]
    public Money TotalAmountMoney => new Money(_totalAmount);
    
    public string? Notes { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public Guid? CreditTermId { get; private set; }
    
    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    // BaseEntity'deki protected RemoveDomainEvent yerine public RemoveDomainEvent kullanılabilir
    // Service layer'dan event kaldırılabilmesi için public yapıldı
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected RemoveDomainEvent'i çağır
        base.RemoveDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.7: Concurrency Control
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Encapsulated collection - Read-only access
    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();
    
    // Navigation properties
    public Organization Organization { get; private set; } = null!;
    public B2BUser? B2BUser { get; private set; }
    public User? ApprovedBy { get; private set; }
    public CreditTerm? CreditTerm { get; private set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private PurchaseOrder() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static PurchaseOrder Create(
        Guid organizationId,
        Guid? b2bUserId,
        string poNumber,
        Organization organization,
        DateTime? expectedDeliveryDate = null,
        Guid? creditTermId = null)
    {
        Guard.AgainstDefault(organizationId, nameof(organizationId));
        Guard.AgainstNullOrEmpty(poNumber, nameof(poNumber));
        Guard.AgainstNull(organization, nameof(organization));

        var purchaseOrder = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            B2BUserId = b2bUserId,
            PONumber = poNumber,
            Organization = organization,
            Status = PurchaseOrderStatus.Draft,
            _subTotal = 0, // EF Core compatibility - backing field
            _tax = 0, // EF Core compatibility - backing field
            _totalAmount = 0, // EF Core compatibility - backing field
            ExpectedDeliveryDate = expectedDeliveryDate,
            CreditTermId = creditTermId,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Event - Purchase Order Created
        purchaseOrder.AddDomainEvent(new PurchaseOrderCreatedEvent(
            purchaseOrder.Id,
            organizationId,
            b2bUserId,
            poNumber,
            0));

        return purchaseOrder;
    }

    // ✅ BOLUM 1.1: Domain Logic - Add item to purchase order
    public void AddItem(Product product, int quantity, Money unitPrice, string? notes = null)
    {
        Guard.AgainstNull(product, nameof(product));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNull(unitPrice, nameof(unitPrice));
        Guard.AgainstNegative(unitPrice.Amount, nameof(unitPrice));

        // ✅ BOLUM 1.1: Business rule - Can only add items to draft orders
        if (Status != PurchaseOrderStatus.Draft)
            throw new DomainException("Sadece taslak durumundaki siparişlere ürün eklenebilir");

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var item = PurchaseOrderItem.Create(
            Id,
            product.Id,
            product,
            quantity,
            unitPrice,
            notes);

        _items.Add(item);
        RecalculateTotals(); // ValidateInvariants() içinde çağrılıyor
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Purchase Order Item Added
        // Not: PurchaseOrderItem aggregate içinde entity olduğu için ayrı event'e gerek yok
        // Ancak PurchaseOrder'ın toplam tutarı değiştiği için event ekleniyor
        AddDomainEvent(new PurchaseOrderItemAddedEvent(Id, product.Id, quantity, unitPrice.Amount));
    }

    // ✅ BOLUM 1.1: Domain Logic - Remove item from purchase order
    public void RemoveItem(Guid purchaseOrderItemId)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new DomainException("Sadece taslak durumundaki siparişlerden ürün çıkarılabilir");

        var item = _items.FirstOrDefault(i => i.Id == purchaseOrderItemId);
        if (item == null)
            throw new DomainException("Sipariş öğesi bulunamadı");

        var productId = item.ProductId;
        _items.Remove(item);
        RecalculateTotals(); // ValidateInvariants() içinde çağrılıyor
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Purchase Order Item Removed
        // Not: PurchaseOrderItem aggregate içinde entity olduğu için ayrı event'e gerek yok
        // Ancak PurchaseOrder'ın toplam tutarı değiştiği için event ekleniyor
        AddDomainEvent(new PurchaseOrderItemRemovedEvent(Id, productId, purchaseOrderItemId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update item quantity in purchase order
    public void UpdateItemQuantity(Guid purchaseOrderItemId, int newQuantity)
    {
        Guard.AgainstNegativeOrZero(newQuantity, nameof(newQuantity));

        if (Status != PurchaseOrderStatus.Draft)
            throw new DomainException("Sadece taslak durumundaki siparişlerde ürün miktarı değiştirilebilir");

        var item = _items.FirstOrDefault(i => i.Id == purchaseOrderItemId);
        if (item == null)
            throw new DomainException("Sipariş öğesi bulunamadı");

        var oldQuantity = item.Quantity;
        item.UpdateQuantity(newQuantity);
        RecalculateTotals(); // ValidateInvariants() içinde çağrılıyor
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Purchase Order Item Updated
        // Not: PurchaseOrderItem aggregate içinde entity olduğu için ayrı event'e gerek yok
        // Ancak PurchaseOrder'ın toplam tutarı değiştiği için event ekleniyor
        AddDomainEvent(new PurchaseOrderItemUpdatedEvent(Id, item.ProductId, purchaseOrderItemId, oldQuantity, newQuantity));
    }

    // ✅ BOLUM 1.1: Domain Logic - Calculate totals
    private void RecalculateTotals()
    {
        _subTotal = _items.Sum(i => i.TotalPrice);
        // Tax will be calculated externally based on tax rate
        _totalAmount = _subTotal + _tax;
        
        // ✅ BOLUM 1.6: Invariant validation - Validate after recalculation
        ValidateInvariants();
    }
    
    // ✅ BOLUM 1.6: Invariant validation - Total amount must be non-negative
    private void ValidateInvariants()
    {
        if (_totalAmount < 0)
            throw new DomainException("Sipariş tutarı negatif olamaz");

        if (_items.Count == 0 && Status != PurchaseOrderStatus.Cancelled)
            throw new DomainException("Sipariş en az bir ürün içermelidir");
    }

    // ✅ BOLUM 1.1: Domain Logic - Set tax amount
    public void SetTax(Money taxAmount)
    {
        Guard.AgainstNull(taxAmount, nameof(taxAmount));
        Guard.AgainstNegative(taxAmount.Amount, nameof(taxAmount));
        
        if (Status != PurchaseOrderStatus.Draft)
            throw new DomainException("Sadece taslak durumundaki siparişlerde vergi güncellenebilir");

        _tax = taxAmount.Amount;
        _totalAmount = _subTotal + _tax;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.6: Invariant validation - Validate after setting tax
        ValidateInvariants();
    }

    // ✅ BOLUM 1.1: State Transition - Submit purchase order
    public void Submit()
    {
        // ✅ BOLUM 1.6: Invariant validation
        if (Status != PurchaseOrderStatus.Draft)
            throw new DomainException("Sadece taslak durumundaki siparişler gönderilebilir");

        if (_items.Count == 0)
            throw new DomainException("Boş sipariş gönderilemez");

        if (TotalAmount <= 0)
            throw new DomainException("Toplam tutar pozitif olmalıdır");

        Status = PurchaseOrderStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Purchase Order Submitted
        AddDomainEvent(new PurchaseOrderSubmittedEvent(
            Id,
            OrganizationId,
            PONumber,
            TotalAmount));
    }

    // ✅ BOLUM 1.1: State Transition - Approve purchase order
    public void Approve(Guid approvedByUserId)
    {
        Guard.AgainstDefault(approvedByUserId, nameof(approvedByUserId));

        if (Status != PurchaseOrderStatus.Submitted)
            throw new DomainException("Sadece gönderilmiş siparişler onaylanabilir");

        Status = PurchaseOrderStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedByUserId = approvedByUserId;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Purchase Order Approved
        AddDomainEvent(new PurchaseOrderApprovedEvent(
            Id,
            OrganizationId,
            approvedByUserId,
            PONumber,
            TotalAmount));
    }

    // ✅ BOLUM 1.1: State Transition - Reject purchase order
    public void Reject(string reason)
    {
        Guard.AgainstNullOrEmpty(reason, nameof(reason));

        if (Status != PurchaseOrderStatus.Submitted)
            throw new DomainException("Sadece gönderilmiş siparişler reddedilebilir");

        Status = PurchaseOrderStatus.Rejected;
        Notes = string.IsNullOrWhiteSpace(Notes) 
            ? $"Red Sebebi: {reason}" 
            : $"{Notes}\nRed Sebebi: {reason}";
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Purchase Order Rejected
        AddDomainEvent(new PurchaseOrderRejectedEvent(
            Id,
            OrganizationId,
            PONumber,
            reason));
    }

    // ✅ BOLUM 1.1: State Transition - Cancel purchase order
    public void Cancel()
    {
        if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.Submitted)
            throw new DomainException("Sadece taslak veya gönderilmiş siparişler iptal edilebilir");

        Status = PurchaseOrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - Purchase Order Cancelled
        AddDomainEvent(new PurchaseOrderCancelledEvent(
            Id,
            OrganizationId,
            PONumber));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update notes
    public void UpdateNotes(string notes)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new DomainException("Sadece taslak durumundaki siparişlerde notlar güncellenebilir");

        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}

