using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.ValueObjects;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;

namespace Merge.Domain.Entities;

/// <summary>
/// Review aggregate root - Rich Domain Model implementation
/// </summary>
public class Review : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - Rating (1-5 arası)
    private int _rating;
    
    // ✅ BOLUM 1.4: Invariant validation - Rating 1-5 arası
    public int Rating 
    { 
        get => _rating; 
        private set 
        { 
            Guard.AgainstOutOfRange(value, 1, 5, nameof(Rating));
            _rating = value;
        } 
    }
    
    public string Title { get; private set; } = string.Empty;
    public string Comment { get; private set; } = string.Empty;
    public bool IsVerifiedPurchase { get; private set; } = false;
    public bool IsApproved { get; private set; } = false;
    public int HelpfulCount { get; private set; } = 0;
    public int UnhelpfulCount { get; private set; } = 0;

    // ✅ BOLUM 1.3: Value Object property
    [NotMapped]
    public Rating RatingValueObject => new Rating(_rating);

    // Navigation properties
    public User User { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public ICollection<ReviewHelpfulness> HelpfulnessVotes { get; private set; } = new List<ReviewHelpfulness>();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Review() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Review Create(
        Guid userId,
        Guid productId,
        Rating rating,
        string title,
        string comment,
        bool isVerifiedPurchase = false)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNull(rating, nameof(rating));
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstNullOrEmpty(comment, nameof(comment));

        var review = new Review
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            _rating = rating.Value,
            Title = title,
            Comment = comment,
            IsVerifiedPurchase = isVerifiedPurchase,
            IsApproved = false,
            HelpfulCount = 0,
            UnhelpfulCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        return review;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update rating
    public void UpdateRating(Rating newRating)
    {
        Guard.AgainstNull(newRating, nameof(newRating));
        _rating = newRating.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update title
    public void UpdateTitle(string newTitle)
    {
        Guard.AgainstNullOrEmpty(newTitle, nameof(newTitle));
        Title = newTitle;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update comment
    public void UpdateComment(string newComment)
    {
        Guard.AgainstNullOrEmpty(newComment, nameof(newComment));
        Comment = newComment;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Approve review
    public void Approve()
    {
        if (IsApproved)
            return;

        IsApproved = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Reject review
    public void Reject()
    {
        if (!IsApproved)
            return;

        IsApproved = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as helpful
    public void MarkAsHelpful()
    {
        HelpfulCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as unhelpful
    public void MarkAsUnhelpful()
    {
        UnhelpfulCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set verified purchase
    public void SetVerifiedPurchase(bool isVerified)
    {
        IsVerifiedPurchase = isVerified;
        UpdatedAt = DateTime.UtcNow;
    }
}
