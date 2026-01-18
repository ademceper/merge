using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.CreateEmailTemplate;

public record CreateEmailTemplateCommand(
    string Name,
    string Description,
    string Subject,
    string HtmlContent,
    string TextContent,
    string Type,
    List<string>? Variables,
    string? Thumbnail) : IRequest<EmailTemplateDto>;
