using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.ML.Queries.GetPriceRecommendation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPriceRecommendationQuery(Guid ProductId) : IRequest<PriceRecommendationDto>;
