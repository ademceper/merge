using System.Text.Json.Serialization;

namespace Merge.Application.DTOs.Support;

/// <summary>
/// Customer Communication DTO with HATEOAS links
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record CustomerCommunicationDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string CommunicationType,
    string Channel,
    string Subject,
    string Content,
    string Direction,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    Guid? SentByUserId,
    string? SentByName,
    string? RecipientEmail,
    string? RecipientPhone,
    string Status,
    DateTime? SentAt,
    DateTime? DeliveredAt,
    DateTime? ReadAt,
    string? ErrorMessage,
    /// Typed DTO (Over-posting korumasi)
    CustomerCommunicationSettingsDto? Metadata,
    DateTime CreatedAt,
    [property: JsonPropertyName("_links")]
    Dictionary<string, object>? Links = null
);
