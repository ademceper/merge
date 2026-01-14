using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Modules.Identity;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ReviewHelpfulness Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Review aggregate'ine ait child entity
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class ReviewHelpfulness : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid ReviewId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsHelpful { get; private set; } // true = helpful, false = not helpful

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Review Review { get; private set; } = null!;
    public User User { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ReviewHelpfulness() { }

    // ✅ BOLUM 1.1: Factory Method with validation
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

        // ✅ BOLUM 1.4: Invariant validation
        reviewHelpfulness.ValidateInvariants();

        // ✅ BOLUM 1.5: Domain Events - ReviewHelpfulnessMarkedEvent
        reviewHelpfulness.AddDomainEvent(new ReviewHelpfulnessMarkedEvent(
            reviewHelpfulness.ReviewId,
            reviewHelpfulness.UserId,
            reviewHelpfulness.IsHelpful));

        return reviewHelpfulness;
    }

    // ✅ BOLUM 1.1: Domain Method - Update helpfulness vote
    public void UpdateVote(bool newIsHelpful)
    {
        if (IsHelpful == newIsHelpful)
            return;

        IsHelpful = newIsHelpful;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();

        // ✅ BOLUM 1.5: Domain Events - ReviewHelpfulnessMarkedEvent
        AddDomainEvent(new ReviewHelpfulnessMarkedEvent(ReviewId, UserId, IsHelpful));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
    }

    // ✅ BOLUM 1.4: Invariant validation
    private void ValidateInvariants()
    {
        if (Guid.Empty == ReviewId)
            throw new DomainException("Review ID boş olamaz");

        if (Guid.Empty == UserId)
            throw new DomainException("Kullanıcı ID boş olamaz");
    }
}
