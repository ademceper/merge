using Merge.Domain.Enums;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Entities;

/// <summary>
/// SellerApplication Entity - Rich Domain Model implementation
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class SellerApplication : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public string BusinessName { get; private set; } = string.Empty;
    // ✅ ARCHITECTURE: Enum kullanımı (string BusinessType yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public BusinessType BusinessType { get; private set; }
    public string TaxNumber { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string BankName { get; private set; } = string.Empty;
    public string BankAccountNumber { get; private set; } = string.Empty;
    public string BankAccountHolderName { get; private set; } = string.Empty;
    public string IBAN { get; private set; } = string.Empty;
    public string BusinessDescription { get; private set; } = string.Empty;
    public string ProductCategories { get; private set; } = string.Empty; // JSON array
    public decimal EstimatedMonthlyRevenue { get; private set; }
    public SellerApplicationStatus Status { get; private set; } = SellerApplicationStatus.Pending;
    public string? RejectionReason { get; private set; }
    public string? AdditionalNotes { get; private set; }
    public Guid? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    // Document URLs
    public string? IdentityDocumentUrl { get; private set; }
    public string? TaxCertificateUrl { get; private set; }
    public string? BankStatementUrl { get; private set; }
    public string? BusinessLicenseUrl { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public User? Reviewer { get; private set; }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private SellerApplication() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    // ✅ BOLUM 1.3: Value Objects - Email, PhoneNumber, IBAN, Address validation
    public static SellerApplication Create(
        Guid userId,
        string businessName,
        BusinessType businessType,
        string taxNumber,
        string address,
        string city,
        string country,
        string postalCode,
        string phoneNumber,
        string email,
        string bankName,
        string bankAccountNumber,
        string bankAccountHolderName,
        string iban,
        string businessDescription,
        string productCategories,
        decimal estimatedMonthlyRevenue,
        string? identityDocumentUrl = null,
        string? taxCertificateUrl = null,
        string? bankStatementUrl = null,
        string? businessLicenseUrl = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(businessName, nameof(businessName));
        Guard.AgainstNullOrEmpty(taxNumber, nameof(taxNumber));
        Guard.AgainstNullOrEmpty(address, nameof(address));
        Guard.AgainstNullOrEmpty(city, nameof(city));
        Guard.AgainstNullOrEmpty(country, nameof(country));
        Guard.AgainstNullOrEmpty(postalCode, nameof(postalCode));
        Guard.AgainstNegative(estimatedMonthlyRevenue, nameof(estimatedMonthlyRevenue));

        // ✅ BOLUM 1.3: Value Objects - Validation using Value Objects
        var emailValueObject = new Email(email);
        var phoneNumberValueObject = new PhoneNumber(phoneNumber);
        var ibanValueObject = new IBAN(iban);
        var addressValueObject = new Merge.Domain.ValueObjects.Address(address, city, country, postalCode);

        var application = new SellerApplication
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BusinessName = businessName,
            BusinessType = businessType,
            TaxNumber = taxNumber,
            Address = addressValueObject.AddressLine1,
            City = addressValueObject.City,
            Country = addressValueObject.Country,
            PostalCode = addressValueObject.PostalCode,
            PhoneNumber = phoneNumberValueObject.Value,
            Email = emailValueObject.Value,
            BankName = bankName,
            BankAccountNumber = bankAccountNumber,
            BankAccountHolderName = bankAccountHolderName,
            IBAN = ibanValueObject.Value,
            BusinessDescription = businessDescription,
            ProductCategories = productCategories,
            EstimatedMonthlyRevenue = estimatedMonthlyRevenue,
            IdentityDocumentUrl = identityDocumentUrl,
            TaxCertificateUrl = taxCertificateUrl,
            BankStatementUrl = bankStatementUrl,
            BusinessLicenseUrl = businessLicenseUrl,
            Status = SellerApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Event - SellerApplication Created
        application.AddDomainEvent(new SellerApplicationCreatedEvent(application.Id, userId, businessName));

        return application;
    }

    // ✅ BOLUM 1.1: Domain Method - Submit application
    public void Submit()
    {
        if (Status != SellerApplicationStatus.Pending)
            throw new DomainException("Başvuru zaten gönderilmiş");

        Status = SellerApplicationStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - SellerApplication Submitted
        AddDomainEvent(new SellerApplicationSubmittedEvent(Id, UserId));
    }

    // ✅ BOLUM 1.1: Domain Method - Review application
    public void Review(Guid reviewedBy, string? additionalNotes = null)
    {
        Guard.AgainstDefault(reviewedBy, nameof(reviewedBy));

        if (Status != SellerApplicationStatus.Submitted && Status != SellerApplicationStatus.Pending)
            throw new DomainException("Sadece gönderilmiş başvurular incelenebilir");

        ReviewedBy = reviewedBy;
        ReviewedAt = DateTime.UtcNow;
        AdditionalNotes = additionalNotes;
        Status = SellerApplicationStatus.UnderReview;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - SellerApplication Reviewed
        AddDomainEvent(new SellerApplicationReviewedEvent(Id, UserId, reviewedBy));
    }

    // ✅ BOLUM 1.1: Domain Method - Approve application
    public void Approve(Guid approvedBy)
    {
        Guard.AgainstDefault(approvedBy, nameof(approvedBy));

        if (Status == SellerApplicationStatus.Approved)
            throw new DomainException("Başvuru zaten onaylanmış");

        if (Status == SellerApplicationStatus.Rejected)
            throw new DomainException("Reddedilmiş başvuru onaylanamaz");

        Status = SellerApplicationStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ReviewedBy = approvedBy;
        ReviewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - SellerApplication Approved
        AddDomainEvent(new SellerApplicationApprovedEvent(Id, UserId, approvedBy));
    }

    // ✅ BOLUM 1.1: Domain Method - Reject application
    public void Reject(Guid rejectedBy, string rejectionReason)
    {
        Guard.AgainstDefault(rejectedBy, nameof(rejectedBy));
        Guard.AgainstNullOrEmpty(rejectionReason, nameof(rejectionReason));

        if (Status == SellerApplicationStatus.Rejected)
            throw new DomainException("Başvuru zaten reddedilmiş");

        if (Status == SellerApplicationStatus.Approved)
            throw new DomainException("Onaylanmış başvuru reddedilemez");

        Status = SellerApplicationStatus.Rejected;
        RejectionReason = rejectionReason;
        ReviewedBy = rejectedBy;
        ReviewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - SellerApplication Rejected
        AddDomainEvent(new SellerApplicationRejectedEvent(Id, UserId, rejectedBy, rejectionReason));
    }

    // ✅ BOLUM 1.1: Domain Method - Update documents
    public void UpdateDocuments(
        string? identityDocumentUrl = null,
        string? taxCertificateUrl = null,
        string? bankStatementUrl = null,
        string? businessLicenseUrl = null)
    {
        if (Status != SellerApplicationStatus.Pending && Status != SellerApplicationStatus.Submitted)
            throw new DomainException("Sadece bekleyen veya gönderilmiş başvuruların belgeleri güncellenebilir");

        if (identityDocumentUrl != null)
            IdentityDocumentUrl = identityDocumentUrl;

        if (taxCertificateUrl != null)
            TaxCertificateUrl = taxCertificateUrl;

        if (bankStatementUrl != null)
            BankStatementUrl = bankStatementUrl;

        if (businessLicenseUrl != null)
            BusinessLicenseUrl = businessLicenseUrl;

        UpdatedAt = DateTime.UtcNow;
    }
}
