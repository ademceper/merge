using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.CreateEmailCampaign;

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
