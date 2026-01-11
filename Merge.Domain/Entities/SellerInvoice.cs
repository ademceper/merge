using Merge.Domain.Enums;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// SellerInvoice Entity - Rich Domain Model implementation
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class SellerInvoice : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid SellerId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty; // Auto-generated: INV-YYYYMM-XXXXXX
    public DateTime InvoiceDate { get; private set; } = DateTime.UtcNow;
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public decimal TotalEarnings { get; private set; }
    public decimal TotalCommissions { get; private set; }
    public decimal TotalPayouts { get; private set; }
    public decimal PlatformFees { get; private set; }
    public decimal NetAmount { get; private set; }
    public SellerInvoiceStatus Status { get; private set; } = SellerInvoiceStatus.Draft;
    public DateTime? PaidAt { get; private set; }
    public string? Notes { get; private set; }
    public string? InvoiceData { get; private set; } // JSON for invoice items
    
    // Navigation properties
    public User Seller { get; private set; } = null!;

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private SellerInvoice() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static SellerInvoice Create(
        Guid sellerId,
        string invoiceNumber,
        DateTime periodStart,
        DateTime periodEnd,
        decimal totalEarnings,
        decimal totalCommissions,
        decimal totalPayouts,
        decimal platformFees,
        decimal netAmount,
        string? notes = null,
        string? invoiceData = null)
    {
        Guard.AgainstDefault(sellerId, nameof(sellerId));
        Guard.AgainstNullOrEmpty(invoiceNumber, nameof(invoiceNumber));
        Guard.AgainstNegative(totalEarnings, nameof(totalEarnings));
        Guard.AgainstNegative(totalCommissions, nameof(totalCommissions));
        Guard.AgainstNegative(totalPayouts, nameof(totalPayouts));
        Guard.AgainstNegative(platformFees, nameof(platformFees));
        Guard.AgainstNegative(netAmount, nameof(netAmount));

        if (periodEnd <= periodStart)
            throw new DomainException("Dönem bitiş tarihi başlangıç tarihinden sonra olmalıdır");

        var invoice = new SellerInvoice
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            InvoiceNumber = invoiceNumber,
            InvoiceDate = DateTime.UtcNow,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            TotalEarnings = totalEarnings,
            TotalCommissions = totalCommissions,
            TotalPayouts = totalPayouts,
            PlatformFees = platformFees,
            NetAmount = netAmount,
            Notes = notes,
            InvoiceData = invoiceData,
            Status = SellerInvoiceStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Event - SellerInvoice Created
        invoice.AddDomainEvent(new SellerInvoiceCreatedEvent(invoice.Id, sellerId, invoiceNumber, netAmount));

        return invoice;
    }

    // ✅ BOLUM 1.1: Domain Method - Send invoice
    public void Send()
    {
        if (Status != SellerInvoiceStatus.Draft)
            throw new DomainException("Sadece taslak faturalar gönderilebilir");

        Status = SellerInvoiceStatus.Sent;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - SellerInvoice Sent
        AddDomainEvent(new SellerInvoiceSentEvent(Id, SellerId, InvoiceNumber));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark invoice as paid
    public void MarkAsPaid()
    {
        if (Status == SellerInvoiceStatus.Paid)
            throw new DomainException("Fatura zaten ödenmiş");

        if (Status == SellerInvoiceStatus.Cancelled)
            throw new DomainException("İptal edilmiş fatura ödenemez");
        
        if (Status != SellerInvoiceStatus.Sent)
            throw new DomainException("Sadece gönderilmiş faturalar ödenebilir");

        Status = SellerInvoiceStatus.Paid;
        PaidAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - SellerInvoice Paid
        AddDomainEvent(new SellerInvoicePaidEvent(Id, SellerId, NetAmount));
    }

    // ✅ BOLUM 1.1: Domain Method - Cancel invoice
    public void Cancel(string? reason = null)
    {
        if (Status == SellerInvoiceStatus.Paid)
            throw new DomainException("Ödenmiş fatura iptal edilemez");

        if (Status == SellerInvoiceStatus.Cancelled)
            throw new DomainException("Fatura zaten iptal edilmiş");

        Status = SellerInvoiceStatus.Cancelled;
        if (!string.IsNullOrEmpty(reason))
            Notes = string.IsNullOrEmpty(Notes) ? reason : $"{Notes}\nİptal nedeni: {reason}";
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - SellerInvoice Cancelled
        AddDomainEvent(new SellerInvoiceCancelledEvent(Id, SellerId, InvoiceNumber, reason));
    }

    // ✅ BOLUM 1.1: Domain Method - Update notes
    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update invoice data
    public void UpdateInvoiceData(string? invoiceData)
    {
        InvoiceData = invoiceData;
        UpdatedAt = DateTime.UtcNow;
    }
}
