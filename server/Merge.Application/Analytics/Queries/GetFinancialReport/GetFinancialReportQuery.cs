using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetFinancialReport;

public record GetFinancialReportQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<FinancialReportDto>;

