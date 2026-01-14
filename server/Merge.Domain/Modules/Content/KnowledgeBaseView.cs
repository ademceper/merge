using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Content;

/// <summary>
/// KnowledgeBaseView Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class KnowledgeBaseView : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid ArticleId { get; private set; }
    public Guid? UserId { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public int ViewDuration { get; private set; } = 0; // Seconds

    // Navigation properties - EF Core requires setters, but we keep them private for encapsulation
    public KnowledgeBaseArticle Article { get; private set; } = null!;
    public User? User { get; private set; }

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private KnowledgeBaseView() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static KnowledgeBaseView Create(
        Guid articleId,
        Guid? userId = null,
        string? ipAddress = null,
        string? userAgent = null,
        int viewDuration = 0)
    {
        Guard.AgainstDefault(articleId, nameof(articleId));
        Guard.AgainstNegative(viewDuration, nameof(viewDuration));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MaxIpAddressLength=45, MaxUserAgentLength=500
        if (!string.IsNullOrEmpty(ipAddress))
            Guard.AgainstLength(ipAddress, 45, nameof(ipAddress));
        if (!string.IsNullOrEmpty(userAgent))
            Guard.AgainstLength(userAgent, 500, nameof(userAgent));

        return new KnowledgeBaseView
        {
            Id = Guid.NewGuid(),
            ArticleId = articleId,
            UserId = userId,
            IpAddress = ipAddress ?? string.Empty,
            UserAgent = userAgent ?? string.Empty,
            ViewDuration = viewDuration,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Method - Update view duration
    public void UpdateViewDuration(int duration)
    {
        Guard.AgainstNegative(duration, nameof(duration));
        ViewDuration = duration;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Restore deleted view
    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

