using MediatR;
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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AddProductToComparisonCommandHandler : IRequestHandler<AddProductToComparisonCommand, ProductComparisonDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<AddProductToComparisonCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_USER_COMPARISON = "user_comparison_";
    private const string CACHE_KEY_USER_COMPARISONS = "user_comparisons_";
    private const string CACHE_KEY_COMPARISON_BY_ID = "comparison_by_id_";
    private const string CACHE_KEY_COMPARISON_MATRIX = "comparison_matrix_";

    public AddProductToComparisonCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper,
        ILogger<AddProductToComparisonCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ProductComparisonDto> Handle(AddProductToComparisonCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding product to comparison. UserId: {UserId}, ProductId: {ProductId}",
            request.UserId, request.ProductId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Get or create user's current comparison
            var comparison = await _context.Set<ProductComparison>()
                .Include(c => c.Items)
                .Where(c => c.UserId == request.UserId && !c.IsSaved)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (comparison == null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
                comparison = ProductComparison.Create(
                    request.UserId,
                    "Current Comparison",
                    false);
                await _context.Set<ProductComparison>().AddAsync(comparison, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Verify product exists and is active
            var product = await _context.Set<ProductEntity>()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null || !product.IsActive)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            // Domain method içinde zaten duplicate check ve max 10 products check var
            comparison.AddProduct(request.ProductId, comparison.Items.Count);

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISON}{request.UserId}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_ID}{comparison.Id}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_COMPARISON_MATRIX}{comparison.Id}", cancellationToken);

            // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (nested ThenInclude)
            comparison = await _context.Set<ProductComparison>()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.Id == comparison.Id, cancellationToken);

            _logger.LogInformation("Product added to comparison successfully. ComparisonId: {ComparisonId}, ProductId: {ProductId}",
                comparison!.Id, request.ProductId);

            return await MapToDto(comparison, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product to comparison. UserId: {UserId}, ProductId: {ProductId}",
                request.UserId, request.ProductId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<ProductComparisonDto> MapToDto(ProductComparison comparison, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (ThenInclude)
        var items = await _context.Set<ProductComparisonItem>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Where(i => i.ComparisonId == comparison.Id)
            .OrderBy(i => i.Position)
            .ToListAsync(cancellationToken);

        var productIds = items.Select(i => i.ProductId).ToList();
        Dictionary<Guid, (decimal Rating, int Count)> reviewsDict;
        if (productIds.Any())
        {
            var reviews = await _context.Set<ReviewEntity>()
                .AsNoTracking()
                .Where(r => productIds.Contains(r.ProductId))
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Rating = (decimal)g.Average(r => r.Rating),
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);
            reviewsDict = reviews.ToDictionary(x => x.ProductId, x => (x.Rating, x.Count));
        }
        else
        {
            reviewsDict = new Dictionary<Guid, (decimal Rating, int Count)>();
        }

        // ✅ BOLUM 7.1.5: Records - with expression kullanımı (immutable record'lar için)
        var products = new List<ComparisonProductDto>();
        foreach (var item in items)
        {
            var hasReviewStats = reviewsDict.TryGetValue(item.ProductId, out var stats);
            var compProduct = _mapper.Map<ComparisonProductDto>(item.Product);
            compProduct = compProduct with
            {
                Position = item.Position,
                Rating = hasReviewStats ? (decimal?)stats.Rating : null,
                ReviewCount = hasReviewStats ? stats.Count : 0,
                Specifications = new Dictionary<string, string>().AsReadOnly(),
                Features = new List<string>().AsReadOnly()
            };
            products.Add(compProduct);
        }

        var comparisonDto = _mapper.Map<ProductComparisonDto>(comparison);
        comparisonDto = comparisonDto with { Products = products.AsReadOnly() };
        return comparisonDto;
    }
}
