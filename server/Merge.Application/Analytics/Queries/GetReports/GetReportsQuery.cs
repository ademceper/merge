using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetReports;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetReportsQuery(
    Guid? UserId = null,
    string? Type = null,
    int Page = 1,
    int PageSize = 0
) : IRequest<PagedResult<ReportDto>>;

