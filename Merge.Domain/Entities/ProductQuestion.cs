namespace Merge.Domain.Entities;

public class ProductQuestion : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public string Question { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = false;
    public int AnswerCount { get; set; } = 0;
    public int HelpfulCount { get; set; } = 0;
    public bool HasSellerAnswer { get; set; } = false;

    // Navigation properties
    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<ProductAnswer> Answers { get; set; } = new List<ProductAnswer>();
    public ICollection<QuestionHelpfulness> HelpfulnessVotes { get; set; } = new List<QuestionHelpfulness>();
}

public class ProductAnswer : BaseEntity
{
    public Guid QuestionId { get; set; }
    public Guid UserId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = false;
    public bool IsSellerAnswer { get; set; } = false;
    public bool IsVerifiedPurchase { get; set; } = false;
    public int HelpfulCount { get; set; } = 0;

    // Navigation properties
    public ProductQuestion Question { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<AnswerHelpfulness> HelpfulnessVotes { get; set; } = new List<AnswerHelpfulness>();
}

public class QuestionHelpfulness : BaseEntity
{
    public Guid QuestionId { get; set; }
    public Guid UserId { get; set; }

    // Navigation properties
    public ProductQuestion Question { get; set; } = null!;
    public User User { get; set; } = null!;
}

public class AnswerHelpfulness : BaseEntity
{
    public Guid AnswerId { get; set; }
    public Guid UserId { get; set; }

    // Navigation properties
    public ProductAnswer Answer { get; set; } = null!;
    public User User { get; set; } = null!;
}
