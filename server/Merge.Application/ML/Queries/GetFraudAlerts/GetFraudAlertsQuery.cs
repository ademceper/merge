using MediatR;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;

namespace Merge.Application.ML.Queries.GetFraudAlerts;

public record GetFraudAlertsQuery(
    string? Status = null,
    string? AlertType = null,
    int? MinRiskScore = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<FraudAlertDto>>;
