using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using System.ComponentModel.DataAnnotations;

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
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User? User { get; private set; }
    public Product? ClickedProduct { get; private set; }

    private SearchHistory() { }

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

        searchHistory.ValidateInvariants();

        searchHistory.AddDomainEvent(new SearchRecordedEvent(
            searchHistory.Id,
            userId,
            searchHistory.SearchTerm,
            resultCount,
            userAgent,
            ipAddress));

        return searchHistory;
    }

    public void RecordClick(Guid productId)
    {
        Guard.AgainstDefault(productId, nameof(productId));

        ClickedResult = true;
        ClickedProductId = productId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SearchClickRecordedEvent(
            Id,
            productId,
            UserId,
            SearchTerm));
    }

    private void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace(_searchTerm))
            throw new DomainException("Arama terimi boş olamaz");

        if (_searchTerm.Length > 200)
            throw new DomainException("Arama terimi en fazla 200 karakter olabilir");

        if (_resultCount < 0)
            throw new DomainException("Sonuç sayısı negatif olamaz");

        if (_userAgent != null && _userAgent.Length > 500)
            throw new DomainException("User agent en fazla 500 karakter olabilir");

        if (_ipAddress != null && _ipAddress.Length > 50)
            throw new DomainException("IP adresi en fazla 50 karakter olabilir");
    }
}

