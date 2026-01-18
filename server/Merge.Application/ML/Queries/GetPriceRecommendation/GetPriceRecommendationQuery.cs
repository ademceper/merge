using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.ML.Queries.GetPriceRecommendation;

public record GetPriceRecommendationQuery(Guid ProductId) : IRequest<PriceRecommendationDto>;
