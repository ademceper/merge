using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Search.Queries.GetCompleteRecommendations;

public record GetCompleteRecommendationsQuery(
    Guid UserId
) : IRequest<PersonalizedRecommendationsDto>;
