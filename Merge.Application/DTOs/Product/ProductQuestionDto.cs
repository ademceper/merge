namespace Merge.Application.DTOs.Product;

public class ProductQuestionDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public int AnswerCount { get; set; }
    public int HelpfulCount { get; set; }
    public bool HasSellerAnswer { get; set; }
    public bool HasUserVoted { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductAnswerDto> Answers { get; set; } = new();
}
