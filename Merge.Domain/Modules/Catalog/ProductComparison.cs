using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ProductComparison Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class ProductComparison : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 12.0: Magic Number'ları Constants'a Taşıma (Clean Architecture)
    private static class ValidationConstants
    {
        public const int MaxNameLength = 200;
        public const int MinShareCodeLength = 6;
        public const int MaxProductsInComparison = 10;
        public const int ShareCodeLength = 8; // GenerateShareCode için kullanılan karakter sayısı
    }

    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    
    private string _name = string.Empty;
    public string Name 
    { 
        get => _name; 
        private set 
        {
            Guard.AgainstLength(value, ValidationConstants.MaxNameLength, nameof(Name));
            _name = value ?? string.Empty;
        } 
    }
    public bool IsSaved { get; private set; } = false;
    
    private string? _shareCode;
    public string? ShareCode 
    { 
        get => _shareCode; 
        private set 
        {
            if (!string.IsNullOrEmpty(value))
            {
                Guard.AgainstOutOfRange(value.Length, ValidationConstants.MinShareCodeLength, int.MaxValue, nameof(ShareCode));
            }
            _shareCode = value;
        } 
    }
    
    public DateTime LastAccessedAt { get; private set; } = DateTime.UtcNow;
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    private readonly List<ProductComparisonItem> _items = new();
    public IReadOnlyCollection<ProductComparisonItem> Items => _items.AsReadOnly();
    
    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ProductComparison() { }
    
    // ✅ BOLUM 1.1: Factory Method with validation
    public static ProductComparison Create(
        Guid userId,
        string? name = null,
        bool isSaved = false)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        
        var comparisonName = name ?? "Unnamed Comparison";
        Guard.AgainstLength(comparisonName, ValidationConstants.MaxNameLength, nameof(name));
        
        var comparison = new ProductComparison
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            _name = comparisonName,
            IsSaved = isSaved,
            LastAccessedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        // ✅ BOLUM 1.4: Invariant validation
        comparison.ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events
        comparison.AddDomainEvent(new ProductComparisonCreatedEvent(comparison.Id, userId, name, 0));
        
        return comparison;
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Add product
    public void AddProduct(Guid productId, int position = -1)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        
        if (_items.Any(i => i.ProductId == productId))
        {
            throw new DomainException("Bu ürün zaten karşılaştırmada");
        }
        
        // ✅ BOLUM 12.0: Magic Number'ları Constants'a Taşıma (Clean Architecture)
        if (_items.Count >= ValidationConstants.MaxProductsInComparison)
        {
            throw new DomainException($"Karşılaştırmada en fazla {ValidationConstants.MaxProductsInComparison} ürün olabilir");
        }
        
        var actualPosition = position >= 0 ? position : _items.Count;
        
        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var item = ProductComparisonItem.Create(Id, productId, actualPosition);
        
        // EF Core will set ComparisonId automatically through navigation property
        _items.Add(item);
        LastAccessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductComparisonUpdatedEvent yayınla (ÖNERİLİR)
        // Ürün ekleme önemli bir business event'tir
        AddDomainEvent(new ProductComparisonUpdatedEvent(Id, UserId, _items.Count));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Remove product
    public void RemoveProduct(Guid productId)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
        {
            throw new DomainException("Bu ürün karşılaştırmada bulunamadı");
        }
        
        _items.Remove(item);
        LastAccessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductComparisonUpdatedEvent yayınla (ÖNERİLİR)
        // Ürün çıkarma önemli bir business event'tir
        AddDomainEvent(new ProductComparisonUpdatedEvent(Id, UserId, _items.Count));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Save comparison
    public void Save(string? name = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Guard.AgainstLength(name, ValidationConstants.MaxNameLength, nameof(name));
            Name = name;
        }
        
        IsSaved = true;
        LastAccessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductComparisonUpdatedEvent yayınla (ÖNERİLİR)
        // Karşılaştırma kaydetme önemli bir business event'tir
        AddDomainEvent(new ProductComparisonUpdatedEvent(Id, UserId, _items.Count));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Generate share code
    public void GenerateShareCode()
    {
        // ✅ BOLUM 12.0: Magic Number'ları Constants'a Taşıma (Clean Architecture)
        // Generate a unique share code using ValidationConstants.ShareCodeLength
        var code = Guid.NewGuid().ToString("N")[..ValidationConstants.ShareCodeLength].ToUpperInvariant();
        ShareCode = code;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductComparisonUpdatedEvent yayınla (ÖNERİLİR)
        // Share code oluşturma önemli bir business event'tir
        AddDomainEvent(new ProductComparisonUpdatedEvent(Id, UserId, _items.Count));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Clear share code
    public void ClearShareCode()
    {
        ShareCode = null;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductComparisonUpdatedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductComparisonUpdatedEvent(Id, UserId, _items.Count));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Update last accessed
    public void UpdateLastAccessed()
    {
        LastAccessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // Note: LastAccessedAt güncellemesi için domain event gerekli değil (frequent operation)
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new ProductComparisonDeletedEvent(Id, UserId));
    }

    // ✅ BOLUM 1.4: Invariant validation
    private void ValidateInvariants()
    {
        if (Guid.Empty == UserId)
            throw new DomainException("Kullanıcı ID boş olamaz");

        if (string.IsNullOrWhiteSpace(_name))
            throw new DomainException("Karşılaştırma adı boş olamaz");

        Guard.AgainstLength(_name, ValidationConstants.MaxNameLength, nameof(Name));

        if (!string.IsNullOrEmpty(_shareCode))
        {
            Guard.AgainstOutOfRange(_shareCode.Length, ValidationConstants.MinShareCodeLength, int.MaxValue, nameof(ShareCode));
        }

        if (_items.Count > ValidationConstants.MaxProductsInComparison)
            throw new DomainException($"Karşılaştırmada en fazla {ValidationConstants.MaxProductsInComparison} ürün olabilir");
    }
}

