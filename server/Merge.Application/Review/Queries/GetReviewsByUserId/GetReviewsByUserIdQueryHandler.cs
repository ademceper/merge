using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Queries.GetReviewsByUserId;

public class GetReviewsByUserIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetReviewsByUserIdQueryHandler> logger, IOptions<ReviewSettings> reviewSettings) : IRequestHandler<GetReviewsByUserIdQuery, PagedResult<ReviewDto>>
{
    private readonly ReviewSettings reviewConfig = reviewSettings.Value;

    public async Task<PagedResult<ReviewDto>> Handle(GetReviewsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > reviewConfig.MaxPageSize
            ? reviewConfig.MaxPageSize
            : request.PageSize;

        logger.LogInformation(
            "Fetching reviews for user. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
            request.UserId, page, pageSize);

        var query = context.Set<ReviewEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.UserId == request.UserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        logger.LogInformation(
            "Retrieved {Count} reviews for user {UserId}, page {Page}, pageSize {PageSize}",
            reviews.Count, request.UserId, page, pageSize);

        var reviewDtos = mapper.Map<IEnumerable<ReviewDto>>(reviews);

        return new PagedResult<ReviewDto>
        {
            Items = reviewDtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
