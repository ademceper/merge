using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.AddProductToComparison;

public class AddProductToComparisonCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<AddProductToComparisonCommandHandler> logger,
    ICacheService cache,
    IMapper mapper) : IRequestHandler<AddProductToComparisonCommand, ProductComparisonDto>
{

    private const string CACHE_KEY_USER_COMPARISON = "user_comparison_";
    private const string CACHE_KEY_USER_COMPARISONS = "user_comparisons_";
    private const string CACHE_KEY_COMPARISON_BY_ID = "comparison_by_id_";
    private const string CACHE_KEY_COMPARISON_MATRIX = "comparison_matrix_";

    public async Task<ProductComparisonDto> Handle(AddProductToComparisonCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding product to comparison. UserId: {UserId}, ProductId: {ProductId}",
            request.UserId, request.ProductId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Get or create user's current comparison
            var comparison = await context.Set<ProductComparison>()
                .Include(c => c.Items)
                .Where(c => c.UserId == request.UserId && !c.IsSaved)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (comparison == null)
            {
                comparison = ProductComparison.Create(
                    request.UserId,
                    "Current Comparison",
                    false);
                await context.Set<ProductComparison>().AddAsync(comparison, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Verify product exists and is active
            var product = await context.Set<ProductEntity>()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null || !product.IsActive)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            // Domain method içinde zaten duplicate check ve max 10 products check var
            comparison.AddProduct(request.ProductId, comparison.Items.Count);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISON}{request.UserId}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_ID}{comparison.Id}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_COMPARISON_MATRIX}{comparison.Id}", cancellationToken);

            comparison = await context.Set<ProductComparison>()
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.Id == comparison.Id, cancellationToken);

            logger.LogInformation("Product added to comparison successfully. ComparisonId: {ComparisonId}, ProductId: {ProductId}",
                comparison!.Id, request.ProductId);

            return await MapToDto(comparison, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding product to comparison. UserId: {UserId}, ProductId: {ProductId}",
                request.UserId, request.ProductId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<ProductComparisonDto> MapToDto(ProductComparison comparison, CancellationToken cancellationToken)
    {
        var itemsQuery = context.Set<ProductComparisonItem>()
            .AsNoTracking()
            .Where(i => i.ComparisonId == comparison.Id)
            .OrderBy(i => i.Position);

        var items = await itemsQuery
            .AsSplitQuery()
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .ToListAsync(cancellationToken);

        var productIdsSubquery = from i in itemsQuery select i.ProductId;
        Dictionary<Guid, (decimal Rating, int Count)> reviewsDict;
        var reviews = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => productIdsSubquery.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Rating = (decimal)g.Average(r => r.Rating),
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);
        reviewsDict = reviews.ToDictionary(x => x.ProductId, x => (x.Rating, x.Count));

        List<ComparisonProductDto> products = [];
        foreach (var item in items)
        {
            var hasReviewStats = reviewsDict.TryGetValue(item.ProductId, out var stats);
            var compProduct = mapper.Map<ComparisonProductDto>(item.Product);
            compProduct = compProduct with
            {
                Position = item.Position,
                Rating = hasReviewStats ? (decimal?)stats.Rating : null,
                ReviewCount = hasReviewStats ? stats.Count : 0,
                Specifications = new Dictionary<string, string>().AsReadOnly(),
                Features = Array.Empty<string>()
            };
            products.Add(compProduct);
        }

        var comparisonDto = mapper.Map<ProductComparisonDto>(comparison);
        comparisonDto = comparisonDto with { Products = products.AsReadOnly() };
        return comparisonDto;
    }
}
