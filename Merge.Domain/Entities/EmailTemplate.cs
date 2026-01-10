using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Domain.Entities;

/// <summary>
/// EmailTemplate Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class EmailTemplate : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string HtmlContent { get; private set; } = string.Empty;
    public string TextContent { get; private set; } = string.Empty;
    public EmailTemplateType Type { get; private set; } = EmailTemplateType.Custom;
    public bool IsActive { get; private set; } = true;
    public string? Thumbnail { get; private set; }
    public string? Variables { get; private set; } // JSON array of available variables like {{customer_name}}, {{order_number}}
    public ICollection<EmailCampaign> Campaigns { get; private set; } = new List<EmailCampaign>();

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private EmailTemplate() { }

    // ✅ BOLUM 1.1: Factory Method with validation
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
            Name = name,
            Description = description ?? string.Empty,
            Subject = subject,
            HtmlContent = htmlContent,
            TextContent = textContent ?? string.Empty,
            Type = type,
            Variables = variables,
            Thumbnail = thumbnail,
            IsActive = true
        };

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        template.AddDomainEvent(new EmailTemplateCreatedEvent(template.Id, name, type));

        return template;
    }

    // ✅ BOLUM 1.1: Domain Method - Update details
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
            Name = name;
        }

        if (description != null)
            Description = description;

        if (!string.IsNullOrEmpty(subject))
        {
            Guard.AgainstLength(subject, 200, nameof(subject));
            Subject = subject;
        }

        if (!string.IsNullOrEmpty(htmlContent))
            HtmlContent = htmlContent;

        if (textContent != null)
            TextContent = textContent;

        if (type.HasValue)
            Type = type.Value;

        if (variables != null)
            Variables = variables;

        if (thumbnail != null)
            Thumbnail = thumbnail;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new EmailTemplateUpdatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new EmailTemplateActivatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new EmailTemplateDeactivatedEvent(Id, Name));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        IsActive = false;

        // ✅ BOLUM 1.5: Domain Events (ZORUNLU)
        AddDomainEvent(new EmailTemplateDeletedEvent(Id, Name));
    }
}

