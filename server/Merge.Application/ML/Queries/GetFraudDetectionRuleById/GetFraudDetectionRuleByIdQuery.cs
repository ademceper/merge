using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.ML.Queries.GetFraudDetectionRuleById;

public record GetFraudDetectionRuleByIdQuery(Guid Id) : IRequest<FraudDetectionRuleDto?>;
