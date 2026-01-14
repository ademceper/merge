using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Search.Queries.GetCompleteRecommendations;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCompleteRecommendationsQuery(
    Guid UserId
) : IRequest<PersonalizedRecommendationsDto>;
