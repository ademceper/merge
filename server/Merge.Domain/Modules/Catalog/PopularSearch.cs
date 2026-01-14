using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// PopularSearch Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PopularSearch : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    private string _searchTerm = string.Empty;
    public string SearchTerm 
    { 
        get => _searchTerm; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(SearchTerm));
            Guard.AgainstLength(value, 200, nameof(SearchTerm));
            _searchTerm = value;
        }
    }
    
    // ✅ BOLUM 1.6: Invariant validation - SearchCount >= 0
    private int _searchCount = 0;
    public int SearchCount 
    { 
        get => _searchCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(SearchCount));
            _searchCount = value;
        }
    }
    
    // ✅ BOLUM 1.6: Invariant validation - ClickThroughCount >= 0
    private int _clickThroughCount = 0;
    public int ClickThroughCount 
    { 
        get => _clickThroughCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(ClickThroughCount));
            _clickThroughCount = value;
        }
    }
    
    // ✅ BOLUM 1.6: Invariant validation - ClickThroughRate >= 0 && <= 100
    private decimal _clickThroughRate = 0;
    public decimal ClickThroughRate 
    { 
        get => _clickThroughRate; 
        private set 
        {
            Guard.AgainstOutOfRange(value, 0, 100, nameof(ClickThroughRate));
            _clickThroughRate = value;
        }
    }
    
    public DateTime LastSearchedAt { get; private set; }
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private PopularSearch() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static PopularSearch Create(string searchTerm)
    {
        Guard.AgainstNullOrEmpty(searchTerm, nameof(searchTerm));

        var popularSearch = new PopularSearch
        {
            Id = Guid.NewGuid(),
            SearchTerm = searchTerm.Trim(),
            SearchCount = 1,
            ClickThroughCount = 0,
            ClickThroughRate = 0,
            LastSearchedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.4: Invariant validation
        popularSearch.ValidateInvariants();

        // ✅ BOLUM 1.5: Domain Events - PopularSearchCreatedEvent
        popularSearch.AddDomainEvent(new PopularSearchCreatedEvent(
            popularSearch.Id,
            popularSearch.SearchTerm));

        return popularSearch;
    }

    // ✅ BOLUM 1.1: Domain Method - Increment search count
    public void IncrementSearchCount()
    {
        SearchCount++;
        LastSearchedAt = DateTime.UtcNow;
        RecalculateClickThroughRate();
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();

        // ✅ BOLUM 1.5: Domain Events - PopularSearchUpdatedEvent
        AddDomainEvent(new PopularSearchUpdatedEvent(
            Id,
            SearchTerm,
            SearchCount,
            ClickThroughCount,
            ClickThroughRate));
    }

    // ✅ BOLUM 1.1: Domain Method - Increment click through count
    public void IncrementClickThroughCount()
    {
        ClickThroughCount++;
        RecalculateClickThroughRate();
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();

        // ✅ BOLUM 1.5: Domain Events - PopularSearchUpdatedEvent
        AddDomainEvent(new PopularSearchUpdatedEvent(
            Id,
            SearchTerm,
            SearchCount,
            ClickThroughCount,
            ClickThroughRate));
    }

    // ✅ BOLUM 1.1: Domain Method - Recalculate click through rate
    private void RecalculateClickThroughRate()
    {
        if (SearchCount > 0)
        {
            ClickThroughRate = (decimal)ClickThroughCount / SearchCount * 100;
        }
        else
        {
            ClickThroughRate = 0;
        }
    }

    // ✅ BOLUM 1.4: Invariant validation
    private void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace(_searchTerm))
            throw new DomainException("Arama terimi boş olamaz");

        if (_searchTerm.Length > 200)
            throw new DomainException("Arama terimi en fazla 200 karakter olabilir");

        if (_searchCount < 0)
            throw new DomainException("Arama sayısı negatif olamaz");

        if (_clickThroughCount < 0)
            throw new DomainException("Tıklama sayısı negatif olamaz");

        if (_clickThroughRate < 0 || _clickThroughRate > 100)
            throw new DomainException("Tıklama oranı 0-100 arasında olmalıdır");
    }
}

