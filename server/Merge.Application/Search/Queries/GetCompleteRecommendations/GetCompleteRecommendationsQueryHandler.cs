using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Search.Queries.GetPersonalizedRecommendations;
using Merge.Application.Search.Queries.GetBasedOnViewHistory;
using Merge.Application.Search.Queries.GetTrendingProducts;
using Merge.Application.Search.Queries.GetBestSellers;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Search.Queries.GetCompleteRecommendations;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetCompleteRecommendationsQueryHandler : IRequestHandler<GetCompleteRecommendationsQuery, PersonalizedRecommendationsDto>
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetCompleteRecommendationsQueryHandler> _logger;

    public GetCompleteRecommendationsQueryHandler(
        IMediator mediator,
        ILogger<GetCompleteRecommendationsQueryHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<PersonalizedRecommendationsDto> Handle(GetCompleteRecommendationsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Complete recommendations isteniyor. UserId: {UserId}",
            request.UserId);

        // ✅ BOLUM 2.0: MediatR - Handler içinde diğer query'leri çağır
        var forYouQuery = new GetPersonalizedRecommendationsQuery(request.UserId, 10);
        var basedOnHistoryQuery = new GetBasedOnViewHistoryQuery(request.UserId, 10);
        var trendingQuery = new GetTrendingProductsQuery(7, 10);
        var bestSellersQuery = new GetBestSellersQuery(10);

        var forYou = await _mediator.Send(forYouQuery, cancellationToken);
        var basedOnHistory = await _mediator.Send(basedOnHistoryQuery, cancellationToken);
        var trending = await _mediator.Send(trendingQuery, cancellationToken);
        var bestSellers = await _mediator.Send(bestSellersQuery, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Complete recommendations tamamlandı. UserId: {UserId}, ForYouCount: {ForYouCount}, BasedOnHistoryCount: {BasedOnHistoryCount}, TrendingCount: {TrendingCount}, BestSellersCount: {BestSellersCount}",
            request.UserId, forYou.Count, basedOnHistory.Count, trending.Count, bestSellers.Count);

        return new PersonalizedRecommendationsDto(
            ForYou: forYou,
            BasedOnHistory: basedOnHistory,
            Trending: trending,
            BestSellers: bestSellers
        );
    }
}
