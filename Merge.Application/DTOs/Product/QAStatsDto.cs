using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record QAStatsDto(
    int TotalQuestions,
    int TotalAnswers,
    int UnansweredQuestions,
    int QuestionsWithSellerAnswer,
    decimal AverageAnswersPerQuestion,
    IReadOnlyList<ProductQuestionDto> RecentQuestions,
    IReadOnlyList<ProductQuestionDto> MostHelpfulQuestions
);
