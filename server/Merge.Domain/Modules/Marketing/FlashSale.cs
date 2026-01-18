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
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? BannerImageUrl { get; private set; }
    
    private readonly List<FlashSaleProduct> _flashSaleProducts = [];
    
    public IReadOnlyCollection<FlashSaleProduct> FlashSaleProducts => _flashSaleProducts.AsReadOnly();

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private FlashSale() { }

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

        flashSale.AddDomainEvent(new FlashSaleCreatedEvent(flashSale.Id, flashSale.Title, flashSale.StartDate, flashSale.EndDate));

        return flashSale;
    }

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

        AddDomainEvent(new FlashSaleUpdatedEvent(Id, Title));
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FlashSaleActivatedEvent(Id, Title));
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FlashSaleDeactivatedEvent(Id, Title));
    }

    public bool IsCurrentlyActive()
    {
        return IsActive && DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FlashSaleDeletedEvent(Id, Title));
    }
}
