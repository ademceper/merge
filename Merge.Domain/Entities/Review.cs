namespace Merge.Domain.Entities;

public class Review : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Rating { get; set; } // 1-5
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsVerifiedPurchase { get; set; } = false;
    public bool IsApproved { get; set; } = false;
    public int HelpfulCount { get; set; } = 0;
    public int UnhelpfulCount { get; set; } = 0;

    // Navigation properties
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ICollection<ReviewHelpfulness> HelpfulnessVotes { get; set; } = new List<ReviewHelpfulness>();
}

