using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetEmailTemplateById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetEmailTemplateByIdQuery(Guid Id) : IRequest<EmailTemplateDto?>;
