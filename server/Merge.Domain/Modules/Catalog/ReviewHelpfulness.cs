using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Modules.Identity;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;


public class ReviewHelpfulness : BaseEntity
{
    public Guid ReviewId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsHelpful { get; private set; } // true = helpful, false = not helpful

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Review Review { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private ReviewHelpfulness() { }

    public static ReviewHelpfulness Create(
        Guid reviewId,
        Guid userId,
        bool isHelpful)
    {
        Guard.AgainstDefault(reviewId, nameof(reviewId));
        Guard.AgainstDefault(userId, nameof(userId));

        var reviewHelpfulness = new ReviewHelpfulness
        {
            Id = Guid.NewGuid(),
            ReviewId = reviewId,
            UserId = userId,
            IsHelpful = isHelpful,
            CreatedAt = DateTime.UtcNow
        };

        reviewHelpfulness.ValidateInvariants();

        reviewHelpfulness.AddDomainEvent(new ReviewHelpfulnessMarkedEvent(
            reviewHelpfulness.ReviewId,
            reviewHelpfulness.UserId,
            reviewHelpfulness.IsHelpful));

        return reviewHelpfulness;
    }

    public void UpdateVote(bool newIsHelpful)
    {
        if (IsHelpful == newIsHelpful)
            return;

        IsHelpful = newIsHelpful;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new ReviewHelpfulnessMarkedEvent(ReviewId, UserId, IsHelpful));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    private void ValidateInvariants()
    {
        if (Guid.Empty == ReviewId)
            throw new DomainException("Review ID boş olamaz");

        if (Guid.Empty == UserId)
            throw new DomainException("Kullanıcı ID boş olamaz");
    }
}
