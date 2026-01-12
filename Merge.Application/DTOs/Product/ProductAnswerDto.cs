using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record ProductAnswerDto(
    Guid Id,
    Guid QuestionId,
    Guid UserId,
    string UserName,
    string Answer,
    bool IsApproved,
    bool IsSellerAnswer,
    bool IsVerifiedPurchase,
    int HelpfulCount,
    bool HasUserVoted,
    DateTime CreatedAt
);
