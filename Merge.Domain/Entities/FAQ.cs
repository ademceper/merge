using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Entities;

/// <summary>
/// FAQ Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class FAQ : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Question { get; private set; } = string.Empty;
    public string Answer { get; private set; } = string.Empty;
    public string Category { get; private set; } = "General";
    public int SortOrder { get; private set; } = 0;
    public int ViewCount { get; private set; } = 0;
    public bool IsPublished { get; private set; } = true;

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private FAQ() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static FAQ Create(
        string question,
        string answer,
        string category = "General",
        int sortOrder = 0,
        bool isPublished = true)
    {
        Guard.AgainstNullOrEmpty(question, nameof(question));
        Guard.AgainstNullOrEmpty(answer, nameof(answer));
        Guard.AgainstNullOrEmpty(category, nameof(category));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxFaqQuestionLength=500, MaxFaqAnswerLength=5000
        Guard.AgainstLength(question, 500, nameof(question));
        Guard.AgainstLength(answer, 5000, nameof(answer));
        Guard.AgainstLength(category, 50, nameof(category));

        var faq = new FAQ
        {
            Id = Guid.NewGuid(),
            Question = question,
            Answer = answer,
            Category = category,
            SortOrder = sortOrder,
            ViewCount = 0,
            IsPublished = isPublished,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - FaqCreatedEvent
        faq.AddDomainEvent(new FaqCreatedEvent(faq.Id, question, category));

        return faq;
    }

    // ✅ BOLUM 1.1: Domain Method - Update question and answer
    public void Update(string question, string answer)
    {
        Guard.AgainstNullOrEmpty(question, nameof(question));
        Guard.AgainstNullOrEmpty(answer, nameof(answer));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxFaqQuestionLength=500, MaxFaqAnswerLength=5000
        Guard.AgainstLength(question, 500, nameof(question));
        Guard.AgainstLength(answer, 5000, nameof(answer));

        Question = question;
        Answer = answer;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update category
    public void UpdateCategory(string category)
    {
        Guard.AgainstNullOrEmpty(category, nameof(category));
        Guard.AgainstLength(category, 50, nameof(category));

        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Publish/Unpublish
    public void SetPublished(bool isPublished)
    {
        IsPublished = isPublished;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update sort order
    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Increment view count
    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new DomainException("FAQ zaten silinmiş");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - FaqDeletedEvent
        AddDomainEvent(new FaqDeletedEvent(Id, Question, Category));
    }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        base.AddDomainEvent(domainEvent);
    }
}

