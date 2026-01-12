using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// FlashSale Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class FlashSale : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? BannerImageUrl { get; private set; }
    
    // Navigation properties
    public ICollection<FlashSaleProduct> FlashSaleProducts { get; private set; } = new List<FlashSaleProduct>();

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private FlashSale() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static FlashSale Create(
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        string? bannerImageUrl = null)
    {
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstNullOrEmpty(description, nameof(description));

        if (startDate >= endDate)
            throw new DomainException("Başlangıç tarihi bitiş tarihinden önce olmalıdır");

        if (startDate < DateTime.UtcNow)
            throw new DomainException("Başlangıç tarihi geçmişte olamaz");

        var flashSale = new FlashSale
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            BannerImageUrl = bannerImageUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - FlashSaleCreatedEvent
        flashSale.AddDomainEvent(new FlashSaleCreatedEvent(flashSale.Id, flashSale.Title, flashSale.StartDate, flashSale.EndDate));

        return flashSale;
    }

    // ✅ BOLUM 1.1: Domain Method - Update details
    public void UpdateDetails(
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        string? bannerImageUrl = null)
    {
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstNullOrEmpty(description, nameof(description));

        if (startDate >= endDate)
            throw new DomainException("Başlangıç tarihi bitiş tarihinden önce olmalıdır");

        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        BannerImageUrl = bannerImageUrl;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - FlashSaleUpdatedEvent
        AddDomainEvent(new FlashSaleUpdatedEvent(Id, Title));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - FlashSaleActivatedEvent
        AddDomainEvent(new FlashSaleActivatedEvent(Id, Title));
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate
    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - FlashSaleDeactivatedEvent
        AddDomainEvent(new FlashSaleDeactivatedEvent(Id, Title));
    }

    // ✅ BOLUM 1.1: Domain Method - Check if flash sale is active
    public bool IsCurrentlyActive()
    {
        return IsActive && DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
