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
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public bool IsSaved { get; private set; } = false;
    
    private string? _shareCode;
    public string? ShareCode 
    { 
        get => _shareCode; 
        private set 
        {
            if (!string.IsNullOrEmpty(value) && value.Length < 6)
            {
                throw new DomainException("Share code en az 6 karakter olmalıdır");
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
        
        var comparison = new ProductComparison
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name ?? "Unnamed Comparison",
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
        
        // Max 10 products in comparison
        if (_items.Count >= 10)
        {
            throw new DomainException("Karşılaştırmada en fazla 10 ürün olabilir");
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
        // Generate a unique 8-character share code
        var code = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
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

        if (string.IsNullOrWhiteSpace(Name))
            throw new DomainException("Karşılaştırma adı boş olamaz");

        if (!string.IsNullOrEmpty(_shareCode) && _shareCode.Length < 6)
            throw new DomainException("Share code en az 6 karakter olmalıdır");

        if (_items.Count > 10)
            throw new DomainException("Karşılaştırmada en fazla 10 ürün olabilir");
    }
}

