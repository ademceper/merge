namespace Merge.Application.DTOs.Product;

public class ProductAnswerDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public bool IsSellerAnswer { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public int HelpfulCount { get; set; }
    public bool HasUserVoted { get; set; }
    public DateTime CreatedAt { get; set; }
}
