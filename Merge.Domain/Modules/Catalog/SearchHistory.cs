using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// SearchHistory Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SearchHistory : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid? UserId { get; private set; } // Nullable for anonymous users
    
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
    
    // ✅ BOLUM 1.6: Invariant validation - ResultCount >= 0
    private int _resultCount = 0;
    public int ResultCount 
    { 
        get => _resultCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(ResultCount));
            _resultCount = value;
        }
    }
    
    public bool ClickedResult { get; private set; } = false;
    public Guid? ClickedProductId { get; private set; }
    
    private string? _userAgent;
    public string? UserAgent 
    { 
        get => _userAgent; 
        private set 
        {
            if (value != null)
            {
                Guard.AgainstLength(value, 500, nameof(UserAgent));
            }
            _userAgent = value;
        }
    }
    
    private string? _ipAddress;
    public string? IpAddress 
    { 
        get => _ipAddress; 
        private set 
        {
            if (value != null)
            {
                Guard.AgainstLength(value, 50, nameof(IpAddress));
            }
            _ipAddress = value;
        }
    }

    // Navigation properties
    public User? User { get; private set; }
    public Product? ClickedProduct { get; private set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private SearchHistory() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static SearchHistory Create(
        Guid? userId,
        string searchTerm,
        int resultCount,
        string? userAgent = null,
        string? ipAddress = null)
    {
        Guard.AgainstNullOrEmpty(searchTerm, nameof(searchTerm));
        Guard.AgainstNegative(resultCount, nameof(resultCount));

        var searchHistory = new SearchHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SearchTerm = searchTerm.Trim(),
            ResultCount = resultCount,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - SearchRecordedEvent
        searchHistory.AddDomainEvent(new SearchRecordedEvent(
            searchHistory.Id,
            userId,
            searchHistory.SearchTerm,
            resultCount,
            userAgent,
            ipAddress));

        return searchHistory;
    }

    // ✅ BOLUM 1.1: Domain Method - Record click
    public void RecordClick(Guid productId)
    {
        Guard.AgainstDefault(productId, nameof(productId));

        ClickedResult = true;
        ClickedProductId = productId;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - SearchClickRecordedEvent
        AddDomainEvent(new SearchClickRecordedEvent(
            Id,
            productId,
            UserId,
            SearchTerm));
    }
}

