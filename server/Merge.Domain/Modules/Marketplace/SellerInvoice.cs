using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Marketplace;

/// <summary>
/// SellerInvoice Entity - Rich Domain Model implementation
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class SellerInvoice : BaseEntity, IAggregateRoot
{
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

    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    private SellerInvoice() { }

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

        invoice.AddDomainEvent(new SellerInvoiceCreatedEvent(invoice.Id, sellerId, invoiceNumber, netAmount));

        return invoice;
    }

    public void Send()
    {
        if (Status != SellerInvoiceStatus.Draft)
            throw new DomainException("Sadece taslak faturalar gönderilebilir");

        Status = SellerInvoiceStatus.Sent;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SellerInvoiceSentEvent(Id, SellerId, InvoiceNumber));
    }

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

        AddDomainEvent(new SellerInvoicePaidEvent(Id, SellerId, NetAmount));
    }

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

        AddDomainEvent(new SellerInvoiceCancelledEvent(Id, SellerId, InvoiceNumber, reason));
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInvoiceData(string? invoiceData)
    {
        InvoiceData = invoiceData;
        UpdatedAt = DateTime.UtcNow;
    }
}
