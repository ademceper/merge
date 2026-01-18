using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Search.Queries.GetPersonalizedRecommendations;
using Merge.Application.Search.Queries.GetBasedOnViewHistory;
using Merge.Application.Search.Queries.GetTrendingProducts;
using Merge.Application.Search.Queries.GetBestSellers;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Search.Queries.GetCompleteRecommendations;

public class GetCompleteRecommendationsQueryHandler(IMediator mediator, ILogger<GetCompleteRecommendationsQueryHandler> logger) : IRequestHandler<GetCompleteRecommendationsQuery, PersonalizedRecommendationsDto>
{

    public async Task<PersonalizedRecommendationsDto> Handle(GetCompleteRecommendationsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Complete recommendations isteniyor. UserId: {UserId}",
            request.UserId);

        var forYouQuery = new GetPersonalizedRecommendationsQuery(request.UserId, 10);
        var basedOnHistoryQuery = new GetBasedOnViewHistoryQuery(request.UserId, 10);
        var trendingQuery = new GetTrendingProductsQuery(7, 10);
        var bestSellersQuery = new GetBestSellersQuery(10);

        var forYou = await mediator.Send(forYouQuery, cancellationToken);
        var basedOnHistory = await mediator.Send(basedOnHistoryQuery, cancellationToken);
        var trending = await mediator.Send(trendingQuery, cancellationToken);
        var bestSellers = await mediator.Send(bestSellersQuery, cancellationToken);

        logger.LogInformation(
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
