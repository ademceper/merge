using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Catalog;

namespace Merge.Domain.Modules.Support;

/// <summary>
/// AnswerHelpfulness Entity - Rich Domain Model implementation
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class AnswerHelpfulness : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid AnswerId { get; private set; }
    public Guid UserId { get; private set; }

    // Navigation properties
    public ProductAnswer Answer { get; private set; } = null!;
    public User User { get; private set; } = null!;
    
    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private AnswerHelpfulness() { }
    
    // ✅ BOLUM 1.1: Factory Method with validation
    public static AnswerHelpfulness Create(
        Guid answerId,
        Guid userId)
    {
        Guard.AgainstDefault(answerId, nameof(answerId));
        Guard.AgainstDefault(userId, nameof(userId));
        
        var answerHelpfulness = new AnswerHelpfulness
        {
            Id = Guid.NewGuid(),
            AnswerId = answerId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        
        // ✅ BOLUM 1.5: Domain Events - AnswerHelpfulnessMarkedEvent
        answerHelpfulness.AddDomainEvent(new AnswerHelpfulnessMarkedEvent(
            answerHelpfulness.AnswerId,
            answerHelpfulness.UserId));
        
        return answerHelpfulness;
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

