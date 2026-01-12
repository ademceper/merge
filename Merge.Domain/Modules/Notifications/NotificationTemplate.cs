using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Notifications;

/// <summary>
/// NotificationTemplate Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class NotificationTemplate : BaseEntity
{
    private string _name = string.Empty;
    public string Name 
    { 
        get => _name; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Name));
            Guard.AgainstLength(value, 100, nameof(Name));
            _name = value;
        }
    }
    
    private string _description = string.Empty;
    public string Description 
    { 
        get => _description; 
        private set 
        {
            Guard.AgainstLength(value, 500, nameof(Description));
            _description = value ?? string.Empty;
        }
    }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Type YASAK)
    public NotificationType Type { get; private set; }
    
    private string _titleTemplate = string.Empty;
    public string TitleTemplate 
    { 
        get => _titleTemplate; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(TitleTemplate));
            Guard.AgainstLength(value, 200, nameof(TitleTemplate));
            _titleTemplate = value;
        }
    }
    
    private string _messageTemplate = string.Empty;
    public string MessageTemplate 
    { 
        get => _messageTemplate; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(MessageTemplate));
            Guard.AgainstLength(value, 2000, nameof(MessageTemplate));
            _messageTemplate = value;
        }
    }
    
    private string? _linkTemplate;
    public string? LinkTemplate 
    { 
        get => _linkTemplate; 
        private set 
        {
            if (value != null)
            {
                Guard.AgainstLength(value, 500, nameof(LinkTemplate));
            }
            _linkTemplate = value;
        }
    }
    
    public bool IsActive { get; private set; } = true;
    
    private string? _variables;
    public string? Variables 
    { 
        get => _variables; 
        private set 
        {
            if (value != null)
            {
                Guard.AgainstLength(value, 2000, nameof(Variables));
            }
            _variables = value;
        }
    }
    
    private string? _defaultData;
    public string? DefaultData 
    { 
        get => _defaultData; 
        private set 
        {
            if (value != null)
            {
                Guard.AgainstLength(value, 5000, nameof(DefaultData));
            }
            _defaultData = value;
        }
    }
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private NotificationTemplate() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static NotificationTemplate Create(
        string name,
        NotificationType type,
        string titleTemplate,
        string messageTemplate,
        string? description = null,
        string? linkTemplate = null,
        bool isActive = true,
        string? variables = null,
        string? defaultData = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstLength(name, 100, nameof(name));
        Guard.AgainstNullOrEmpty(titleTemplate, nameof(titleTemplate));
        Guard.AgainstLength(titleTemplate, 200, nameof(titleTemplate));
        Guard.AgainstNullOrEmpty(messageTemplate, nameof(messageTemplate));
        Guard.AgainstLength(messageTemplate, 2000, nameof(messageTemplate));
        if (description != null)
        {
            Guard.AgainstLength(description, 500, nameof(description));
        }
        if (linkTemplate != null)
        {
            Guard.AgainstLength(linkTemplate, 500, nameof(linkTemplate));
        }
        if (variables != null)
        {
            Guard.AgainstLength(variables, 2000, nameof(variables));
        }
        if (defaultData != null)
        {
            Guard.AgainstLength(defaultData, 5000, nameof(defaultData));
        }

        return new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            _name = name,
            _description = description ?? string.Empty,
            Type = type,
            _titleTemplate = titleTemplate,
            _messageTemplate = messageTemplate,
            _linkTemplate = linkTemplate,
            IsActive = isActive,
            _variables = variables,
            _defaultData = defaultData,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Method - Update template
    public void Update(
        string? name = null,
        string? description = null,
        NotificationType? type = null,
        string? titleTemplate = null,
        string? messageTemplate = null,
        string? linkTemplate = null,
        bool? isActive = null,
        string? variables = null,
        string? defaultData = null)
    {
        if (name != null)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstLength(name, 100, nameof(name));
            _name = name;
        }
        if (description != null)
        {
            Guard.AgainstLength(description, 500, nameof(description));
            _description = description;
        }
        if (type.HasValue)
        {
            Type = type.Value;
        }
        if (titleTemplate != null)
        {
            Guard.AgainstNullOrEmpty(titleTemplate, nameof(titleTemplate));
            Guard.AgainstLength(titleTemplate, 200, nameof(titleTemplate));
            _titleTemplate = titleTemplate;
        }
        if (messageTemplate != null)
        {
            Guard.AgainstNullOrEmpty(messageTemplate, nameof(messageTemplate));
            Guard.AgainstLength(messageTemplate, 2000, nameof(messageTemplate));
            _messageTemplate = messageTemplate;
        }
        if (linkTemplate != null)
        {
            Guard.AgainstLength(linkTemplate, 500, nameof(linkTemplate));
            _linkTemplate = linkTemplate;
        }
        if (isActive.HasValue)
        {
            IsActive = isActive.Value;
        }
        if (variables != null)
        {
            Guard.AgainstLength(variables, 2000, nameof(variables));
            _variables = variables;
        }
        if (defaultData != null)
        {
            Guard.AgainstLength(defaultData, 5000, nameof(defaultData));
            _defaultData = defaultData;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Activate template
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate template
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Delete template (soft delete)
    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Şablon zaten silinmiş");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

