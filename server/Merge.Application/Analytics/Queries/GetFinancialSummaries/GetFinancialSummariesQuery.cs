using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetFinancialSummaries;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetFinancialSummariesQuery(
    DateTime StartDate,
    DateTime EndDate,
    string Period
) : IRequest<List<FinancialSummaryDto>>;

