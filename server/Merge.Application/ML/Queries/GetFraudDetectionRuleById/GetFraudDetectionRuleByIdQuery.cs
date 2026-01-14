using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.ML.Queries.GetFraudDetectionRuleById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetFraudDetectionRuleByIdQuery(Guid Id) : IRequest<FraudDetectionRuleDto?>;
