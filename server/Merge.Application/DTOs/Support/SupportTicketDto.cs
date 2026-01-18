using System.Text.Json.Serialization;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Support;

/// <summary>
/// Support Ticket DTO with HATEOAS links
/// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
/// </summary>
public record SupportTicketDto(
    Guid Id,
    string TicketNumber,
    Guid UserId,
    string UserName,
    string UserEmail,
    string Category,
    string Priority,
    string Status,
    string Subject,
    string Description,
    Guid? OrderId,
    string? OrderNumber,
    Guid? ProductId,
    string? ProductName,
    Guid? AssignedToId,
    string? AssignedToName,
    DateTime? ResolvedAt,
    DateTime? ClosedAt,
    int ResponseCount,
    DateTime? LastResponseAt,
    DateTime CreatedAt,
    IReadOnlyList<TicketMessageDto> Messages,
    IReadOnlyList<TicketAttachmentDto> Attachments,
    [property: JsonPropertyName("_links")]
    Dictionary<string, object>? Links = null
);
