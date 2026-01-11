namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record ProductQuestionDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid UserId,
    string UserName,
    string Question,
    bool IsApproved,
    int AnswerCount,
    int HelpfulCount,
    bool HasSellerAnswer,
    bool HasUserVoted,
    DateTime CreatedAt,
    IReadOnlyList<ProductAnswerDto> Answers
);
