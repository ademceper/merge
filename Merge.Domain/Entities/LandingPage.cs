using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Domain.Entities;

/// <summary>
/// LandingPage Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class LandingPage : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty; // JSON or HTML content
    public string? Template { get; private set; } // Template identifier
    public ContentStatus Status { get; private set; } = ContentStatus.Draft;
    public Guid? AuthorId { get; private set; }
    public User? Author { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? StartDate { get; private set; } // When to start showing
    public DateTime? EndDate { get; private set; } // When to stop showing
    public bool IsActive { get; private set; } = true;
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public string? OgImageUrl { get; private set; }
    public int ViewCount { get; private set; } = 0;
    public int ConversionCount { get; private set; } = 0; // Track conversions
    public decimal ConversionRate { get; private set; } = 0; // Percentage
    public bool EnableABTesting { get; private set; } = false;
    public Guid? VariantOfId { get; private set; } // If this is a variant for A/B testing
    public LandingPage? VariantOf { get; private set; }
    public ICollection<LandingPage> Variants { get; private set; } = new List<LandingPage>();
    public int TrafficSplit { get; private set; } = 50; // Percentage of traffic for A/B testing

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private LandingPage() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static LandingPage Create(
        string name,
        string title,
        string content,
        Guid? authorId = null,
        string? template = null,
        ContentStatus status = ContentStatus.Draft,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? metaTitle = null,
        string? metaDescription = null,
        string? ogImageUrl = null,
        bool enableABTesting = false,
        Guid? variantOfId = null,
        int trafficSplit = 50,
        string? slug = null) // Optional slug for uniqueness handling
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstNullOrEmpty(content, nameof(content));
        Guard.AgainstNegative(trafficSplit, nameof(trafficSplit));
        if (trafficSplit > 100)
            throw new DomainException("Traffic split cannot exceed 100%");

        if (startDate.HasValue && endDate.HasValue && startDate.Value >= endDate.Value)
        {
            throw new DomainException("Start date must be before end date");
        }

        var finalSlug = slug ?? GenerateSlug(name);

        var landingPage = new LandingPage
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = finalSlug,
            Title = title,
            Content = content,
            Template = template,
            Status = status,
            AuthorId = authorId,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            OgImageUrl = ogImageUrl,
            EnableABTesting = enableABTesting,
            VariantOfId = variantOfId,
            TrafficSplit = trafficSplit,
            PublishedAt = status == ContentStatus.Published ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events (ÖNERİLİR)
        landingPage.AddDomainEvent(new LandingPageCreatedEvent(
            landingPage.Id,
            landingPage.Name,
            landingPage.Slug,
            landingPage.AuthorId ?? Guid.Empty));

        return landingPage;
    }

    // ✅ BOLUM 1.1: Domain Methods - Business logic encapsulation
    public void UpdateName(string name)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Name = name;
        Slug = GenerateSlug(name);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new LandingPageUpdatedEvent(Id, Name, Slug));
    }

    public void UpdateTitle(string title)
    {
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Title = title;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new LandingPageUpdatedEvent(Id, Name, Slug));
    }

    public void UpdateContent(string content)
    {
        Guard.AgainstNullOrEmpty(content, nameof(content));
        Content = content;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new LandingPageUpdatedEvent(Id, Name, Slug));
    }

    public void UpdateTemplate(string? template)
    {
        Template = template;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new LandingPageUpdatedEvent(Id, Name, Slug));
    }

    public void UpdateStatus(ContentStatus status)
    {
        if (Status == status) return;

        Status = status;
        UpdatedAt = DateTime.UtcNow;

        if (status == ContentStatus.Published && !PublishedAt.HasValue)
        {
            PublishedAt = DateTime.UtcNow;
            AddDomainEvent(new LandingPagePublishedEvent(Id, Name, Slug, AuthorId ?? Guid.Empty));
        }
        else
        {
            AddDomainEvent(new LandingPageUpdatedEvent(Id, Name, Slug));
        }
    }

    public void UpdateSchedule(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && startDate.Value >= endDate.Value)
        {
            throw new DomainException("Start date must be before end date");
        }

        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new LandingPageUpdatedEvent(Id, Name, Slug));
    }

    public void UpdateMetaInformation(string? metaTitle, string? metaDescription, string? ogImageUrl)
    {
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        OgImageUrl = ogImageUrl;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new LandingPageUpdatedEvent(Id, Name, Slug));
    }

    public void UpdateABTestingSettings(bool enableABTesting, int trafficSplit)
    {
        Guard.AgainstNegative(trafficSplit, nameof(trafficSplit));
        if (trafficSplit > 100)
            throw new DomainException("Traffic split cannot exceed 100%");

        EnableABTesting = enableABTesting;
        TrafficSplit = trafficSplit;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new LandingPageUpdatedEvent(Id, Name, Slug));
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new LandingPageUpdatedEvent(Id, Name, Slug));
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new LandingPageUpdatedEvent(Id, Name, Slug));
    }

    public void Publish()
    {
        if (Status == ContentStatus.Published) return;

        Status = ContentStatus.Published;
        PublishedAt = DateTime.UtcNow;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LandingPagePublishedEvent(Id, Name, Slug, AuthorId ?? Guid.Empty));
    }

    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void TrackConversion()
    {
        ConversionCount++;
        if (ViewCount > 0)
        {
            ConversionRate = (decimal)ConversionCount / ViewCount * 100;
        }
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events (ÖNERİLİR)
        AddDomainEvent(new LandingPageConversionTrackedEvent(Id, ConversionCount, ConversionRate));
    }

    public LandingPage CreateVariant(
        string name,
        string title,
        string content,
        string? template = null,
        ContentStatus status = ContentStatus.Draft,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? metaTitle = null,
        string? metaDescription = null,
        string? ogImageUrl = null,
        int trafficSplit = 50)
    {
        if (!EnableABTesting)
            throw new DomainException("A/B testing must be enabled to create variants");

        var variant = Create(
            name: name,
            title: title,
            content: content,
            authorId: AuthorId,
            template: template ?? Template,
            status: status,
            startDate: startDate ?? StartDate,
            endDate: endDate ?? EndDate,
            metaTitle: metaTitle ?? MetaTitle,
            metaDescription: metaDescription ?? MetaDescription,
            ogImageUrl: ogImageUrl ?? OgImageUrl,
            enableABTesting: true,
            variantOfId: Id,
            trafficSplit: trafficSplit,
            slug: $"{Slug}-variant-{DateTime.UtcNow.Ticks}");

        return variant;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new LandingPageDeletedEvent(Id, Name, Slug));
    }

    // ✅ BOLUM 1.1: Helper method for slug generation
    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant()
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace(":", "")
            .Replace(";", "");

        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }
}
