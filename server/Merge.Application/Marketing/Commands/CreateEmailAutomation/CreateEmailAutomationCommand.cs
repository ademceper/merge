using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.CreateEmailAutomation;

public record CreateEmailAutomationCommand(
    string Name,
    string Description,
    string Type,
    Guid TemplateId,
    int DelayHours,
    EmailAutomationSettingsDto? TriggerConditions) : IRequest<EmailAutomationDto>;
