using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

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
