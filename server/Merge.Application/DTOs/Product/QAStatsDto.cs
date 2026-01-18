using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Product;

public record QAStatsDto(
    int TotalQuestions,
    int TotalAnswers,
    int UnansweredQuestions,
    int QuestionsWithSellerAnswer,
    decimal AverageAnswersPerQuestion,
    IReadOnlyList<ProductQuestionDto> RecentQuestions,
    IReadOnlyList<ProductQuestionDto> MostHelpfulQuestions
);
