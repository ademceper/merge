using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Common;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Queries.GetPendingReviews;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetPendingReviewsQueryHandler : IRequestHandler<GetPendingReviewsQuery, PagedResult<ReviewDto>>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<GetPendingReviewsQueryHandler> _logger;

    public GetPendingReviewsQueryHandler(
        IAdminService adminService,
        ILogger<GetPendingReviewsQueryHandler> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<PagedResult<ReviewDto>> Handle(GetPendingReviewsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching pending reviews. Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

        return await _adminService.GetPendingReviewsAsync(request.Page, request.PageSize, cancellationToken);
    }
}

