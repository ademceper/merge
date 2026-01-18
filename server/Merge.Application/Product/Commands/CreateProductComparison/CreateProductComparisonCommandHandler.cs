using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.CreateProductComparison;

public class CreateProductComparisonCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateProductComparisonCommandHandler> logger, ICacheService cache) : IRequestHandler<CreateProductComparisonCommand, ProductComparisonDto>
{

    private const string CACHE_KEY_USER_COMPARISON = "user_comparison_";
    private const string CACHE_KEY_USER_COMPARISONS = "user_comparisons_";

    public async Task<ProductComparisonDto> Handle(CreateProductComparisonCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating product comparison. UserId: {UserId}, ProductCount: {ProductCount}",
            request.UserId, request.ProductIds.Count);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var comparison = ProductComparison.Create(
                request.UserId,
                request.Name,
                !string.IsNullOrEmpty(request.Name));

            await context.Set<ProductComparison>().AddAsync(comparison, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var productIds = request.ProductIds.Distinct().ToList();
            var products = await context.Set<ProductEntity>()
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            int position = 0;
            foreach (var productId in request.ProductIds)
            {
                if (products.ContainsKey(productId))
                {
                    comparison.AddProduct(productId, position++);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISON}{request.UserId}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_true_", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_false_", cancellationToken);

            comparison = await context.Set<ProductComparison>()
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.Id == comparison.Id, cancellationToken);

            logger.LogInformation("Product comparison created successfully. ComparisonId: {ComparisonId}, UserId: {UserId}",
                comparison!.Id, request.UserId);

            // Map to DTO (ProductComparisonService'deki MapToDto mantığını kullan)
            return await MapToDto(comparison, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product comparison. UserId: {UserId}", request.UserId);
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
                Specifications = new Dictionary<string, string>().AsReadOnly(), // TODO: Map from product specifications
                Features = Array.Empty<string>() // TODO: Map from product features
            };
            products.Add(compProduct);
        }

        var comparisonDto = mapper.Map<ProductComparisonDto>(comparison);
        comparisonDto = comparisonDto with { Products = products.AsReadOnly() };
        return comparisonDto;
    }
}
