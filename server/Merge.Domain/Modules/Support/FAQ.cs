using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Domain.Modules.Support;

/// <summary>
/// FAQ Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class FAQ : BaseEntity, IAggregateRoot
{
    public string Question { get; private set; } = string.Empty;
    public string Answer { get; private set; } = string.Empty;
    public string Category { get; private set; } = "General";
    public int SortOrder { get; private set; } = 0;
    public int ViewCount { get; private set; } = 0;
    public bool IsPublished { get; private set; } = true;

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private FAQ() { }

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

        faq.AddDomainEvent(new FaqCreatedEvent(faq.Id, question, category));

        return faq;
    }

    public void Update(string question, string answer)
    {
        Guard.AgainstNullOrEmpty(question, nameof(question));
        Guard.AgainstNullOrEmpty(answer, nameof(answer));
        // Configuration değerleri: MaxFaqQuestionLength=500, MaxFaqAnswerLength=5000
        Guard.AgainstLength(question, 500, nameof(question));
        Guard.AgainstLength(answer, 5000, nameof(answer));

        Question = question;
        Answer = answer;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCategory(string category)
    {
        Guard.AgainstNullOrEmpty(category, nameof(category));
        Guard.AgainstLength(category, 50, nameof(category));

        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPublished(bool isPublished)
    {
        IsPublished = isPublished;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new DomainException("FAQ zaten silinmiş");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FaqDeletedEvent(Id, Question, Category));
    }

    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        base.AddDomainEvent(domainEvent);
    }
}

