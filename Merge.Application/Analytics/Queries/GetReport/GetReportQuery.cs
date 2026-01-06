using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetReport;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetReportQuery(
    Guid Id,
    Guid UserId
) : IRequest<ReportDto?>;

