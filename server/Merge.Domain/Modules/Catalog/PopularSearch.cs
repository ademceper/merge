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
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private PopularSearch() { }

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

        popularSearch.ValidateInvariants();

        popularSearch.AddDomainEvent(new PopularSearchCreatedEvent(
            popularSearch.Id,
            popularSearch.SearchTerm));

        return popularSearch;
    }

    public void IncrementSearchCount()
    {
        SearchCount++;
        LastSearchedAt = DateTime.UtcNow;
        RecalculateClickThroughRate();
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new PopularSearchUpdatedEvent(
            Id,
            SearchTerm,
            SearchCount,
            ClickThroughCount,
            ClickThroughRate));
    }

    public void IncrementClickThroughCount()
    {
        ClickThroughCount++;
        RecalculateClickThroughRate();
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new PopularSearchUpdatedEvent(
            Id,
            SearchTerm,
            SearchCount,
            ClickThroughCount,
            ClickThroughRate));
    }

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

