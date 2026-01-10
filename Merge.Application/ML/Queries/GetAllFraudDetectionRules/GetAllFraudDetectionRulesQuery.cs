using MediatR;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;

namespace Merge.Application.ML.Queries.GetAllFraudDetectionRules;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllFraudDetectionRulesQuery(
    string? RuleType = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<FraudDetectionRuleDto>>;
