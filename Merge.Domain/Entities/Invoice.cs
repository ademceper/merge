using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Domain.Entities;

/// <summary>
/// Invoice Entity - BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class Invoice : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid OrderId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public DateTime InvoiceDate { get; private set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal Tax { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public string? PdfUrl { get; private set; }
    public string? Notes { get; private set; }

    // ✅ BOLUM 1.5: Concurrency Control
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Order Order { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Invoice() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Invoice Create(
        Guid orderId,
        string invoiceNumber,
        DateTime invoiceDate,
        DateTime? dueDate,
        decimal subTotal,
        decimal tax,
        decimal shippingCost,
        decimal discount,
        decimal totalAmount,
        string? notes = null)
    {
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstNullOrEmpty(invoiceNumber, nameof(invoiceNumber));
        Guard.AgainstNegativeOrZero(subTotal, nameof(subTotal));
        Guard.AgainstNegative(tax, nameof(tax));
        Guard.AgainstNegative(shippingCost, nameof(shippingCost));
        Guard.AgainstNegative(discount, nameof(discount));
        Guard.AgainstNegativeOrZero(totalAmount, nameof(totalAmount));

        if (subTotal + tax + shippingCost - discount != totalAmount)
            throw new DomainException("Fatura tutarları tutarsız. Toplam tutar alt toplam, vergi, kargo ve indirim toplamına eşit olmalıdır.");

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            InvoiceNumber = invoiceNumber,
            InvoiceDate = invoiceDate,
            DueDate = dueDate,
            SubTotal = subTotal,
            Tax = tax,
            ShippingCost = shippingCost,
            Discount = discount,
            TotalAmount = totalAmount,
            Status = InvoiceStatus.Draft,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - InvoiceCreatedEvent yayınla
        invoice.AddDomainEvent(new InvoiceCreatedEvent(
            invoice.Id,
            orderId,
            invoiceNumber,
            totalAmount));

        return invoice;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as sent
    public void MarkAsSent()
    {
        if (Status != InvoiceStatus.Draft)
            throw new DomainException("Sadece taslak faturalar gönderilebilir");

        Status = InvoiceStatus.Sent;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - InvoiceSentEvent yayınla
        AddDomainEvent(new InvoiceSentEvent(Id, OrderId, InvoiceNumber));
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as paid
    public void MarkAsPaid()
    {
        if (Status != InvoiceStatus.Sent && Status != InvoiceStatus.Overdue)
            throw new DomainException("Sadece gönderilmiş veya vadesi geçmiş faturalar ödenmiş olarak işaretlenebilir");

        Status = InvoiceStatus.Paid;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - InvoicePaidEvent yayınla
        AddDomainEvent(new InvoicePaidEvent(Id, OrderId, InvoiceNumber, TotalAmount));
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as overdue
    public void MarkAsOverdue()
    {
        if (Status == InvoiceStatus.Paid || Status == InvoiceStatus.Cancelled)
            throw new DomainException("Ödenmiş veya iptal edilmiş faturalar vadesi geçmiş olarak işaretlenemez");

        if (!DueDate.HasValue || DueDate.Value > DateTime.UtcNow)
            throw new DomainException("Fatura vadesi henüz gelmemiş");

        Status = InvoiceStatus.Overdue;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - InvoiceOverdueEvent yayınla
        AddDomainEvent(new InvoiceOverdueEvent(Id, OrderId, InvoiceNumber, DueDate!.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Cancel invoice
    public void Cancel(string? reason = null)
    {
        if (Status == InvoiceStatus.Paid)
            throw new DomainException("Ödenmiş faturalar iptal edilemez");

        Status = InvoiceStatus.Cancelled;
        if (!string.IsNullOrEmpty(reason))
            Notes = string.IsNullOrEmpty(Notes) ? reason : $"{Notes}\nİptal nedeni: {reason}";
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - InvoiceCancelledEvent yayınla
        AddDomainEvent(new InvoiceCancelledEvent(Id, OrderId, InvoiceNumber, reason));
    }

    // ✅ BOLUM 1.1: Domain Logic - Set PDF URL
    public void SetPdfUrl(string pdfUrl)
    {
        Guard.AgainstNullOrEmpty(pdfUrl, nameof(pdfUrl));
        PdfUrl = pdfUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update notes
    public void UpdateNotes(string notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}

