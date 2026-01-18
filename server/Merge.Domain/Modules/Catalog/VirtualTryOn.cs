using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// VirtualTryOn Entity - Rich Domain Model implementation
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class VirtualTryOn : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    
    private string _modelUrl = string.Empty; // AR/3D model URL
    public string ModelUrl 
    { 
        get => _modelUrl; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(ModelUrl));
            if (value.Length > 2000)
            {
                throw new DomainException("Model URL en fazla 2000 karakter olabilir");
            }
            _modelUrl = value;
        } 
    }
    
    private string? _previewImageUrl; // Preview image URL
    public string? PreviewImageUrl 
    { 
        get => _previewImageUrl; 
        private set 
        {
            if (value is not null && value.Length > 2000)
            {
                throw new DomainException("Preview image URL en fazla 2000 karakter olabilir");
            }
            _previewImageUrl = value;
        } 
    }
    
    private string _viewerType = "AR"; // AR, 3D, Image
    public string ViewerType 
    { 
        get => _viewerType; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(ViewerType));
            var validTypes = new[] { "AR", "3D", "Image" };
            if (!validTypes.Contains(value))
            {
                throw new DomainException("Viewer type must be one of: AR, 3D, Image");
            }
            _viewerType = value;
        } 
    }
    
    private string? _configuration; // JSON configuration for viewer
    public string? Configuration 
    { 
        get => _configuration; 
        private set 
        {
            if (value is not null && value.Length > 10000)
            {
                throw new DomainException("Configuration en fazla 10000 karakter olabilir");
            }
            _configuration = value;
        } 
    }
    
    // User measurements for virtual try-on
    private decimal? _height;
    public decimal? Height 
    { 
        get => _height; 
        private set 
        {
            if (value.HasValue)
            {
                Guard.AgainstNegativeOrZero(value.Value, nameof(Height));
                if (value.Value > 300) // Max 300 cm
                {
                    throw new DomainException("Boy en fazla 300 cm olabilir");
                }
            }
            _height = value;
        } 
    }
    
    private decimal? _chest; // User chest measurement
    public decimal? Chest 
    { 
        get => _chest; 
        private set 
        {
            if (value.HasValue)
            {
                Guard.AgainstNegativeOrZero(value.Value, nameof(Chest));
                if (value.Value > 200) // Max 200 cm
                {
                    throw new DomainException("Göğüs ölçüsü en fazla 200 cm olabilir");
                }
            }
            _chest = value;
        } 
    }
    
    private decimal? _waist; // User waist measurement
    public decimal? Waist 
    { 
        get => _waist; 
        private set 
        {
            if (value.HasValue)
            {
                Guard.AgainstNegativeOrZero(value.Value, nameof(Waist));
                if (value.Value > 200) // Max 200 cm
                {
                    throw new DomainException("Bel ölçüsü en fazla 200 cm olabilir");
                }
            }
            _waist = value;
        } 
    }
    
    private decimal? _hips; // User hips measurement
    public decimal? Hips 
    { 
        get => _hips; 
        private set 
        {
            if (value.HasValue)
            {
                Guard.AgainstNegativeOrZero(value.Value, nameof(Hips));
                if (value.Value > 200) // Max 200 cm
                {
                    throw new DomainException("Kalça ölçüsü en fazla 200 cm olabilir");
                }
            }
            _hips = value;
        } 
    }
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Product Product { get; private set; } = null!;
    public User User { get; private set; } = null!;
    
    private VirtualTryOn() { }
    
    public static VirtualTryOn Create(
        Guid productId,
        Guid userId,
        string modelUrl,
        string viewerType = "AR",
        string? previewImageUrl = null,
        string? configuration = null,
        decimal? height = null,
        decimal? chest = null,
        decimal? waist = null,
        decimal? hips = null)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(modelUrl, nameof(modelUrl));
        
        if (modelUrl.Length > 2000)
        {
            throw new DomainException("Model URL en fazla 2000 karakter olabilir");
        }
        
        var validTypes = new[] { "AR", "3D", "Image" };
        if (!validTypes.Contains(viewerType))
        {
            throw new DomainException("Viewer type must be one of: AR, 3D, Image");
        }
        
        var virtualTryOn = new VirtualTryOn
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UserId = userId,
            _modelUrl = modelUrl,
            _viewerType = viewerType,
            _previewImageUrl = previewImageUrl,
            _configuration = configuration,
            _height = height,
            _chest = chest,
            _waist = waist,
            _hips = hips,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };
        
        virtualTryOn.ValidateInvariants();
        
        return virtualTryOn;
    }
    
    public void UpdateModelUrl(string newModelUrl)
    {
        Guard.AgainstNullOrEmpty(newModelUrl, nameof(newModelUrl));
        if (newModelUrl.Length > 2000)
        {
            throw new DomainException("Model URL en fazla 2000 karakter olabilir");
        }
        _modelUrl = newModelUrl;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void UpdatePreviewImageUrl(string? newPreviewImageUrl)
    {
        if (newPreviewImageUrl is not null && newPreviewImageUrl.Length > 2000)
        {
            throw new DomainException("Preview image URL en fazla 2000 karakter olabilir");
        }
        _previewImageUrl = newPreviewImageUrl;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void UpdateViewerType(string newViewerType)
    {
        Guard.AgainstNullOrEmpty(newViewerType, nameof(newViewerType));
        var validTypes = new[] { "AR", "3D", "Image" };
        if (!validTypes.Contains(newViewerType))
        {
            throw new DomainException("Viewer type must be one of: AR, 3D, Image");
        }
        _viewerType = newViewerType;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void UpdateConfiguration(string? newConfiguration)
    {
        if (newConfiguration is not null && newConfiguration.Length > 10000)
        {
            throw new DomainException("Configuration en fazla 10000 karakter olabilir");
        }
        _configuration = newConfiguration;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void UpdateMeasurements(
        decimal? height = null,
        decimal? chest = null,
        decimal? waist = null,
        decimal? hips = null)
    {
        if (height.HasValue) Height = height;
        if (chest.HasValue) Chest = chest;
        if (waist.HasValue) Waist = waist;
        if (hips.HasValue) Hips = hips;
        
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void Enable()
    {
        if (IsEnabled) return;
        
        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void Disable()
    {
        if (!IsEnabled) return;
        
        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    private void ValidateInvariants()
    {
        if (Guid.Empty == ProductId)
            throw new DomainException("Ürün ID boş olamaz");

        if (Guid.Empty == UserId)
            throw new DomainException("Kullanıcı ID boş olamaz");

        if (string.IsNullOrWhiteSpace(_modelUrl))
            throw new DomainException("Model URL boş olamaz");

        if (_modelUrl.Length > 2000)
            throw new DomainException("Model URL en fazla 2000 karakter olabilir");

        var validTypes = new[] { "AR", "3D", "Image" };
        if (!validTypes.Contains(_viewerType))
            throw new DomainException("Viewer type must be one of: AR, 3D, Image");

        if (_height.HasValue && (_height.Value <= 0 || _height.Value > 300))
            throw new DomainException("Boy 0-300 cm arasında olmalıdır");

        if (_chest.HasValue && (_chest.Value <= 0 || _chest.Value > 200))
            throw new DomainException("Göğüs ölçüsü 0-200 cm arasında olmalıdır");

        if (_waist.HasValue && (_waist.Value <= 0 || _waist.Value > 200))
            throw new DomainException("Bel ölçüsü 0-200 cm arasında olmalıdır");

        if (_hips.HasValue && (_hips.Value <= 0 || _hips.Value > 200))
            throw new DomainException("Kalça ölçüsü 0-200 cm arasında olmalıdır");
    }
}

