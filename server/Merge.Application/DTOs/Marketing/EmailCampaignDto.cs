using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Marketing;


public record EmailCampaignDto(
    Guid Id,
    string Name,
    string Subject,
    string FromName,
    string FromEmail,
    string Status,
    string Type,
    DateTime? ScheduledAt,
    DateTime? SentAt,
    string TargetSegment,
    int TotalRecipients,
    int SentCount,
    int DeliveredCount,
    int OpenedCount,
    int ClickedCount,
    decimal OpenRate,
    decimal ClickRate,
    DateTime CreatedAt);
