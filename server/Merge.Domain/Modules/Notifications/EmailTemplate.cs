using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Modules.Marketing;

namespace Merge.Domain.Modules.Notifications;

/// <summary>
/// EmailTemplate Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class EmailTemplate : BaseEntity, IAggregateRoot
{
    private string _name = string.Empty;
    public string Name 
    { 
        get => _name; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Name));
            Guard.AgainstLength(value, 200, nameof(Name));
            _name = value;
        }
    }
    
    public string Description { get; private set; } = string.Empty;
    
    private string _subject = string.Empty;
    public string Subject 
    { 
        get => _subject; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Subject));
            Guard.AgainstLength(value, 200, nameof(Subject));
            _subject = value;
        }
    }
    
    private string _htmlContent = string.Empty;
    public string HtmlContent 
    { 
        get => _htmlContent; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(HtmlContent));
            _htmlContent = value;
        }
    }
    
    public string TextContent { get; private set; } = string.Empty;
    public EmailTemplateType Type { get; private set; } = EmailTemplateType.Custom;
    public bool IsActive { get; private set; } = true;
    
    private string? _thumbnail;
    public string? Thumbnail 
    { 
        get => _thumbnail; 
        private set 
        {
            if (value != null)
            {
                Guard.AgainstLength(value, 500, nameof(Thumbnail));
            }
            _thumbnail = value;
        }
    }
    
    public string? Variables { get; private set; } // JSON array of available variables like {{customer_name}}, {{order_number}}
    public ICollection<EmailCampaign> Campaigns { get; private set; } = [];

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private EmailTemplate() { }

    public static EmailTemplate Create(
        string name,
        string description,
        string subject,
        string htmlContent,
        string textContent,
        EmailTemplateType type,
        string? variables = null,
        string? thumbnail = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(subject, nameof(subject));
        Guard.AgainstNullOrEmpty(htmlContent, nameof(htmlContent));
        Guard.AgainstLength(name, 200, nameof(name));
        Guard.AgainstLength(subject, 200, nameof(subject));

        var template = new EmailTemplate
        {
            _name = name,
            Description = description ?? string.Empty,
            _subject = subject,
            _htmlContent = htmlContent,
            TextContent = textContent ?? string.Empty,
            Type = type,
            Variables = variables,
            _thumbnail = thumbnail,
            IsActive = true
        };

        template.AddDomainEvent(new EmailTemplateCreatedEvent(template.Id, name, type));

        return template;
    }

    public void UpdateDetails(
        string? name = null,
        string? description = null,
        string? subject = null,
        string? htmlContent = null,
        string? textContent = null,
        EmailTemplateType? type = null,
        string? variables = null,
        string? thumbnail = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Guard.AgainstLength(name, 200, nameof(name));
            _name = name;
        }

        if (description != null)
            Description = description;

        if (!string.IsNullOrEmpty(subject))
        {
            Guard.AgainstLength(subject, 200, nameof(subject));
            _subject = subject;
        }

        if (!string.IsNullOrEmpty(htmlContent))
            _htmlContent = htmlContent;

        if (textContent != null)
            TextContent = textContent;

        if (type.HasValue)
            Type = type.Value;

        if (variables != null)
            Variables = variables;

        if (thumbnail != null)
            _thumbnail = thumbnail;

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new EmailTemplateUpdatedEvent(Id, Name));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new EmailTemplateActivatedEvent(Id, Name));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new EmailTemplateDeactivatedEvent(Id, Name));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new EmailTemplateDeletedEvent(Id, Name));
    }
}

