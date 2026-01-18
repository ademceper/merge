namespace Merge.Application.DTOs.Marketing;


public record LoyaltyTransactionDto(
    Guid Id,
    int Points,
    string Type,
    string Description,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    bool IsExpired);
