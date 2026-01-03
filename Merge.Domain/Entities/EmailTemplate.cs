using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// EmailTemplate Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class EmailTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string TextContent { get; set; } = string.Empty;
    public EmailTemplateType Type { get; set; } = EmailTemplateType.Custom;
    public bool IsActive { get; set; } = true;
    public string? Thumbnail { get; set; }
    public string? Variables { get; set; } // JSON array of available variables like {{customer_name}}, {{order_number}}
    public ICollection<EmailCampaign> Campaigns { get; set; } = new List<EmailCampaign>();
}

