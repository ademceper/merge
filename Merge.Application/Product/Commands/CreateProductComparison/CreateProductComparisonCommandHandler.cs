using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Merge.Domain.Entities.Product;
using ReviewEntity = Merge.Domain.Entities.Review;

namespace Merge.Application.Product.Commands.CreateProductComparison;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateProductComparisonCommandHandler : IRequestHandler<CreateProductComparisonCommand, ProductComparisonDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateProductComparisonCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_USER_COMPARISON = "user_comparison_";
    private const string CACHE_KEY_USER_COMPARISONS = "user_comparisons_";

    public CreateProductComparisonCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateProductComparisonCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ProductComparisonDto> Handle(CreateProductComparisonCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product comparison. UserId: {UserId}, ProductCount: {ProductCount}",
            request.UserId, request.ProductIds.Count);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var comparison = ProductComparison.Create(
                request.UserId,
                request.Name,
                !string.IsNullOrEmpty(request.Name));

            await _context.Set<ProductComparison>().AddAsync(comparison, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ PERFORMANCE: Batch load products to avoid N+1 queries
            var productIds = request.ProductIds.Distinct().ToList();
            var products = await _context.Set<ProductEntity>()
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            int position = 0;
            foreach (var productId in request.ProductIds)
            {
                if (products.ContainsKey(productId))
                {
                    comparison.AddProduct(productId, position++);
                }
            }

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISON}{request.UserId}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_true_", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_USER_COMPARISONS}{request.UserId}_false_", cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            comparison = await _context.Set<ProductComparison>()
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.Id == comparison.Id, cancellationToken);

            _logger.LogInformation("Product comparison created successfully. ComparisonId: {ComparisonId}, UserId: {UserId}",
                comparison!.Id, request.UserId);

            // Map to DTO (ProductComparisonService'deki MapToDto mantığını kullan)
            return await MapToDto(comparison, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product comparison. UserId: {UserId}", request.UserId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<ProductComparisonDto> MapToDto(ProductComparison comparison, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var items = await _context.Set<ProductComparisonItem>()
            .AsNoTracking()
            .Include(i => i.Product)
                .ThenInclude(p => p.Category)
            .Where(i => i.ComparisonId == comparison.Id)
            .OrderBy(i => i.Position)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load reviews to avoid N+1 queries
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

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - with expression kullanımı (immutable record'lar için)
        var products = new List<ComparisonProductDto>();

        foreach (var item in items)
        {
            var hasReviewStats = reviewsDict.TryGetValue(item.ProductId, out var stats);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            var compProduct = _mapper.Map<ComparisonProductDto>(item.Product);
            compProduct = compProduct with
            {
                Position = item.Position,
                Rating = hasReviewStats ? (decimal?)stats.Rating : null,
                ReviewCount = hasReviewStats ? stats.Count : 0,
                Specifications = new Dictionary<string, string>().AsReadOnly(), // TODO: Map from product specifications
                Features = new List<string>().AsReadOnly() // TODO: Map from product features
            };
            products.Add(compProduct);
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var comparisonDto = _mapper.Map<ProductComparisonDto>(comparison);
        comparisonDto = comparisonDto with { Products = products.AsReadOnly() };
        return comparisonDto;
    }
}
