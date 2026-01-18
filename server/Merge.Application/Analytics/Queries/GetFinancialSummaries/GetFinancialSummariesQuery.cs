using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetFinancialSummaries;

public record GetFinancialSummariesQuery(
    DateTime StartDate,
    DateTime EndDate,
    string Period
) : IRequest<List<FinancialSummaryDto>>;

