using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.CreateEmailTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateEmailTemplateCommand(
    string Name,
    string Description,
    string Subject,
    string HtmlContent,
    string TextContent,
    string Type,
    List<string>? Variables,
    string? Thumbnail) : IRequest<EmailTemplateDto>;
