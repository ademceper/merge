using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;
using Merge.Domain.Modules.Identity;
using Address = Merge.Domain.ValueObjects.Address;
using Merge.Domain.Modules.Catalog;

namespace Merge.Domain.Modules.Marketplace;

/// <summary>
/// SellerApplication Entity - Rich Domain Model implementation
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class SellerApplication : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public string BusinessName { get; private set; } = string.Empty;
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

    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    private SellerApplication() { }

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

        var emailValueObject = new Email(email);
        var phoneNumberValueObject = new PhoneNumber(phoneNumber);
        var ibanValueObject = new IBAN(iban);
        var addressValueObject = new Address(address, city, country, postalCode);

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

        application.AddDomainEvent(new SellerApplicationCreatedEvent(application.Id, userId, businessName));

        return application;
    }

    public void Submit()
    {
        if (Status != SellerApplicationStatus.Pending)
            throw new DomainException("Başvuru zaten gönderilmiş");

        Status = SellerApplicationStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SellerApplicationSubmittedEvent(Id, UserId));
    }

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

        AddDomainEvent(new SellerApplicationReviewedEvent(Id, UserId, reviewedBy));
    }

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

        AddDomainEvent(new SellerApplicationApprovedEvent(Id, UserId, approvedBy));
    }

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

        AddDomainEvent(new SellerApplicationRejectedEvent(Id, UserId, rejectedBy, rejectionReason));
    }

    public void UpdateDocuments(
        string? identityDocumentUrl = null,
        string? taxCertificateUrl = null,
        string? bankStatementUrl = null,
        string? businessLicenseUrl = null)
    {
        if (Status != SellerApplicationStatus.Pending && Status != SellerApplicationStatus.Submitted)
            throw new DomainException("Sadece bekleyen veya gönderilmiş başvuruların belgeleri güncellenebilir");

        if (identityDocumentUrl is not null)
            IdentityDocumentUrl = identityDocumentUrl;

        if (taxCertificateUrl is not null)
            TaxCertificateUrl = taxCertificateUrl;

        if (bankStatementUrl is not null)
            BankStatementUrl = bankStatementUrl;

        if (businessLicenseUrl is not null)
            BusinessLicenseUrl = businessLicenseUrl;

        UpdatedAt = DateTime.UtcNow;
    }
}
