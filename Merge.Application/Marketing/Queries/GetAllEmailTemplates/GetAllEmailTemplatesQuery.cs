using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetAllEmailTemplates;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public record GetAllEmailTemplatesQuery(string? Type, int PageNumber, int PageSize) : IRequest<PagedResult<EmailTemplateDto>>;
