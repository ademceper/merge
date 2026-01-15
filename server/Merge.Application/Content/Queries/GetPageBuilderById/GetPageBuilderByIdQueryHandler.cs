using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetPageBuilderById;

public class GetPageBuilderByIdQueryHandler(
    IDbContext context,
    Merge.Application.Interfaces.IRepository<PageBuilder> pageBuilderRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<GetPageBuilderByIdQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetPageBuilderByIdQuery, PageBuilderDto?>
{
    private const string CACHE_KEY_PAGE_BY_ID = "page_builder_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<PageBuilderDto?> Handle(GetPageBuilderByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving page builder with Id: {PageBuilderId}, TrackView: {TrackView}", request.Id, request.TrackView);

        var cacheKey = $"{CACHE_KEY_PAGE_BY_ID}{request.Id}";

        var cachedPage = await cache.GetAsync<PageBuilderDto>(cacheKey, cancellationToken);
        if (cachedPage != null && !request.TrackView)
        {
            logger.LogInformation("Cache hit for page builder. PageBuilderId: {PageBuilderId}", request.Id);
            return cachedPage;
        }

        logger.LogInformation("Cache miss for page builder. PageBuilderId: {PageBuilderId}", request.Id);

        var pageBuilder = request.TrackView
            ? await context.Set<PageBuilder>()
            .AsSplitQuery()
                .Include(pb => pb.Author)
                .FirstOrDefaultAsync(pb => pb.Id == request.Id, cancellationToken)
            : await context.Set<PageBuilder>()
                .AsNoTracking()
                .Include(pb => pb.Author)
                .FirstOrDefaultAsync(pb => pb.Id == request.Id, cancellationToken);

        if (pageBuilder == null)
        {
            logger.LogWarning("Page builder not found with Id: {PageBuilderId}", request.Id);
            return null;
        }

        if (request.TrackView && pageBuilder.Status == ContentStatus.Published && pageBuilder.IsActive)
        {
            pageBuilder.IncrementViewCount();
            await pageBuilderRepository.UpdateAsync(pageBuilder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Successfully retrieved page builder {PageBuilderId}", request.Id);

        var pageDto = mapper.Map<PageBuilderDto>(pageBuilder);

        if (!request.TrackView)
        {
            await cache.SetAsync(cacheKey, pageDto, CACHE_EXPIRATION, cancellationToken);
        }

        return pageDto;
    }
}

