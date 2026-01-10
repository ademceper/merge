using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.CreateEmailCampaign;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateEmailCampaignCommand(
    string Name,
    string Subject,
    string FromName,
    string FromEmail,
    string ReplyToEmail,
    Guid? TemplateId,
    string Content,
    string Type,
    DateTime? ScheduledAt,
    string TargetSegment,
    List<string>? Tags) : IRequest<EmailCampaignDto>;
