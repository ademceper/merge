using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// Invoice Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (ZORUNLU - String Status YASAK)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.6: Invariant Validation (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Invoice : BaseEntity, IAggregateRoot
{
    public Guid OrderId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public DateTime InvoiceDate { get; private set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; private set; }
    
    private decimal _subTotal;
    private decimal _tax;
    private decimal _shippingCost;
    private decimal _discount;
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
    
    public decimal ShippingCost 
    { 
        get => _shippingCost; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(ShippingCost));
            _shippingCost = value;
        }
    }
    
    public decimal Discount 
    { 
        get => _discount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(Discount));
            _discount = value;
        }
    }
    
    public decimal TotalAmount 
    { 
        get => _totalAmount; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(TotalAmount));
            _totalAmount = value;
        }
    }
    
    [NotMapped]
    public Money SubTotalMoney => new Money(_subTotal);
    
    [NotMapped]
    public Money TaxMoney => new Money(_tax);
    
    [NotMapped]
    public Money ShippingCostMoney => new Money(_shippingCost);
    
    [NotMapped]
    public Money DiscountMoney => new Money(_discount);
    
    [NotMapped]
    public Money TotalAmountMoney => new Money(_totalAmount);
    
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public string? PdfUrl { get; private set; }
    public string? Notes { get; private set; }

    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    // BaseEntity'deki protected RemoveDomainEvent yerine public RemoveDomainEvent kullanılabilir
    // Service layer'dan event kaldırılabilmesi için public yapıldı
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected RemoveDomainEvent'i çağır
        base.RemoveDomainEvent(domainEvent);
    }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Order Order { get; private set; } = null!;

    private Invoice() { }

    public static Invoice Create(
        Guid orderId,
        string invoiceNumber,
        DateTime invoiceDate,
        DateTime? dueDate,
        Money subTotal,
        Money tax,
        Money shippingCost,
        Money discount,
        Money totalAmount,
        string? notes = null)
    {
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstNullOrEmpty(invoiceNumber, nameof(invoiceNumber));
        Guard.AgainstNull(subTotal, nameof(subTotal));
        Guard.AgainstNull(tax, nameof(tax));
        Guard.AgainstNull(shippingCost, nameof(shippingCost));
        Guard.AgainstNull(discount, nameof(discount));
        Guard.AgainstNull(totalAmount, nameof(totalAmount));
        Guard.AgainstNegativeOrZero(subTotal.Amount, nameof(subTotal));
        Guard.AgainstNegative(tax.Amount, nameof(tax));
        Guard.AgainstNegative(shippingCost.Amount, nameof(shippingCost));
        Guard.AgainstNegative(discount.Amount, nameof(discount));
        Guard.AgainstNegativeOrZero(totalAmount.Amount, nameof(totalAmount));

        if (subTotal.Amount + tax.Amount + shippingCost.Amount - discount.Amount != totalAmount.Amount)
            throw new DomainException("Fatura tutarları tutarsız. Toplam tutar alt toplam, vergi, kargo ve indirim toplamına eşit olmalıdır.");

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            InvoiceNumber = invoiceNumber,
            InvoiceDate = invoiceDate,
            DueDate = dueDate,
            _subTotal = subTotal.Amount, // EF Core compatibility - backing field
            _tax = tax.Amount, // EF Core compatibility - backing field
            _shippingCost = shippingCost.Amount, // EF Core compatibility - backing field
            _discount = discount.Amount, // EF Core compatibility - backing field
            _totalAmount = totalAmount.Amount, // EF Core compatibility - backing field
            Status = InvoiceStatus.Draft,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        invoice.AddDomainEvent(new InvoiceCreatedEvent(
            invoice.Id,
            orderId,
            invoiceNumber,
            totalAmount.Amount));

        return invoice;
    }

    public void MarkAsSent()
    {
        if (Status != InvoiceStatus.Draft)
            throw new DomainException("Sadece taslak faturalar gönderilebilir");

        Status = InvoiceStatus.Sent;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new InvoiceSentEvent(Id, OrderId, InvoiceNumber));
    }

    public void MarkAsPaid()
    {
        if (Status != InvoiceStatus.Sent && Status != InvoiceStatus.Overdue)
            throw new DomainException("Sadece gönderilmiş veya vadesi geçmiş faturalar ödenmiş olarak işaretlenebilir");

        Status = InvoiceStatus.Paid;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new InvoicePaidEvent(Id, OrderId, InvoiceNumber, TotalAmount));
    }

    public void MarkAsOverdue()
    {
        if (Status == InvoiceStatus.Paid || Status == InvoiceStatus.Cancelled)
            throw new DomainException("Ödenmiş veya iptal edilmiş faturalar vadesi geçmiş olarak işaretlenemez");

        if (!DueDate.HasValue || DueDate.Value > DateTime.UtcNow)
            throw new DomainException("Fatura vadesi henüz gelmemiş");

        Status = InvoiceStatus.Overdue;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new InvoiceOverdueEvent(Id, OrderId, InvoiceNumber, DueDate!.Value));
    }

    public void Cancel(string? reason = null)
    {
        if (Status == InvoiceStatus.Paid)
            throw new DomainException("Ödenmiş faturalar iptal edilemez");

        Status = InvoiceStatus.Cancelled;
        if (!string.IsNullOrEmpty(reason))
            Notes = string.IsNullOrEmpty(Notes) ? reason : $"{Notes}\nİptal nedeni: {reason}";
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new InvoiceCancelledEvent(Id, OrderId, InvoiceNumber, reason));
    }

    public void SetPdfUrl(string pdfUrl)
    {
        Guard.AgainstNullOrEmpty(pdfUrl, nameof(pdfUrl));
        PdfUrl = pdfUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}

