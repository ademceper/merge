using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetEmailTemplateById;

public record GetEmailTemplateByIdQuery(Guid Id) : IRequest<EmailTemplateDto?>;
