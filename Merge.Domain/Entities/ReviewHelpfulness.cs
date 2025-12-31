namespace Merge.Domain.Entities;

public class ReviewHelpfulness : BaseEntity
{
    public Guid ReviewId { get; set; }
    public Guid UserId { get; set; }
    public bool IsHelpful { get; set; } // true = helpful, false = not helpful

    // Navigation properties
    public Review Review { get; set; } = null!;
    public User User { get; set; } = null!;
}
