using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.UpdateEmailTemplate;

public record UpdateEmailTemplateCommand(
    Guid Id,
    string? Name,
    string? Description,
    string? Subject,
    string? HtmlContent,
    string? TextContent,
    string? Type,
    List<string>? Variables,
    string? Thumbnail,
    bool? IsActive) : IRequest<EmailTemplateDto>;
