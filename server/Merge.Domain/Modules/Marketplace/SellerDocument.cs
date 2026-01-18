using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.Enums;

namespace Merge.Domain.Modules.Marketplace;

/// <summary>
/// SellerDocument Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class SellerDocument : BaseEntity
{
    public Guid SellerApplicationId { get; private set; }
    public SellerDocumentType DocumentType { get; private set; }
    public string DocumentUrl { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public bool IsVerified { get; private set; } = false;
    public DateTime? VerifiedAt { get; private set; }
    public Guid? VerifiedBy { get; private set; }

    // Navigation properties
    public SellerApplication SellerApplication { get; private set; } = null!;

    private SellerDocument() { }

    public static SellerDocument Create(
        Guid sellerApplicationId,
        SellerDocumentType documentType,
        string documentUrl,
        string fileName,
        long fileSize)
    {
        Guard.AgainstDefault(sellerApplicationId, nameof(sellerApplicationId));
        Guard.AgainstNullOrEmpty(documentUrl, nameof(documentUrl));
        Guard.AgainstNullOrEmpty(fileName, nameof(fileName));
        Guard.AgainstNegativeOrZero(fileSize, nameof(fileSize));

        return new SellerDocument
        {
            Id = Guid.NewGuid(),
            SellerApplicationId = sellerApplicationId,
            DocumentType = documentType,
            DocumentUrl = documentUrl,
            FileName = fileName,
            FileSize = fileSize,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Verify(Guid verifiedBy)
    {
        Guard.AgainstDefault(verifiedBy, nameof(verifiedBy));

        if (IsVerified)
            throw new DomainException("Belge zaten doğrulanmış");

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        VerifiedBy = verifiedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDocumentUrl(string documentUrl)
    {
        Guard.AgainstNullOrEmpty(documentUrl, nameof(documentUrl));

        if (IsVerified)
            throw new DomainException("Doğrulanmış belge güncellenemez");

        DocumentUrl = documentUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsDocumentVerified() => IsVerified;
}

