using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Support.Queries.GetPublishedFaqs;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public record GetPublishedFaqsQuery(
    string? Category = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<FaqDto>>;
