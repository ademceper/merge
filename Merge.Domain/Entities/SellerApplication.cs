using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// SellerApplication Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SellerApplication : BaseEntity
{
    public Guid UserId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty; // Individual, Company, etc.
    public string TaxNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankAccountHolderName { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public string BusinessDescription { get; set; } = string.Empty;
    public string ProductCategories { get; set; } = string.Empty; // JSON array
    public decimal EstimatedMonthlyRevenue { get; set; }
    public SellerApplicationStatus Status { get; set; } = SellerApplicationStatus.Pending;
    public string? RejectionReason { get; set; }
    public string? AdditionalNotes { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Document URLs
    public string? IdentityDocumentUrl { get; set; }
    public string? TaxCertificateUrl { get; set; }
    public string? BankStatementUrl { get; set; }
    public string? BusinessLicenseUrl { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public User? Reviewer { get; set; }
}
