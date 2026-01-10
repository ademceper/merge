namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Loyalty Transaction DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record LoyaltyTransactionDto(
    Guid Id,
    int Points,
    string Type,
    string Description,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    bool IsExpired);
