namespace Merge.Application.DTOs.Product;

public class QAStatsDto
{
    public int TotalQuestions { get; set; }
    public int TotalAnswers { get; set; }
    public int UnansweredQuestions { get; set; }
    public int QuestionsWithSellerAnswer { get; set; }
    public decimal AverageAnswersPerQuestion { get; set; }
    public List<ProductQuestionDto> RecentQuestions { get; set; } = new();
    public List<ProductQuestionDto> MostHelpfulQuestions { get; set; } = new();
}
