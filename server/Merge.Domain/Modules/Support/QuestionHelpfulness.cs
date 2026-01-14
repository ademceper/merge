using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Catalog;

namespace Merge.Domain.Modules.Support;

/// <summary>
/// QuestionHelpfulness Entity - Rich Domain Model implementation
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class QuestionHelpfulness : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid QuestionId { get; private set; }
    public Guid UserId { get; private set; }

    // Navigation properties
    public ProductQuestion Question { get; private set; } = null!;
    public User User { get; private set; } = null!;
    
    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private QuestionHelpfulness() { }
    
    // ✅ BOLUM 1.1: Factory Method with validation
    public static QuestionHelpfulness Create(
        Guid questionId,
        Guid userId)
    {
        Guard.AgainstDefault(questionId, nameof(questionId));
        Guard.AgainstDefault(userId, nameof(userId));
        
        var questionHelpfulness = new QuestionHelpfulness
        {
            Id = Guid.NewGuid(),
            QuestionId = questionId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        
        // ✅ BOLUM 1.5: Domain Events - QuestionHelpfulnessMarkedEvent
        questionHelpfulness.AddDomainEvent(new QuestionHelpfulnessMarkedEvent(
            questionHelpfulness.QuestionId,
            questionHelpfulness.UserId));
        
        return questionHelpfulness;
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

