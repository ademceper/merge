using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.CreateProductBundle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateProductBundleCommandHandler : IRequestHandler<CreateProductBundleCommand, ProductBundleDto>
{
    private readonly Merge.Application.Interfaces.IRepository<ProductBundle> _bundleRepository;
    private readonly Merge.Application.Interfaces.IRepository<BundleItem> _bundleItemRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateProductBundleCommandHandler> _logger;
    private const string CACHE_KEY_BUNDLE_BY_ID = "bundle_";
    private const string CACHE_KEY_ALL_BUNDLES = "bundles_all";
    private const string CACHE_KEY_ACTIVE_BUNDLES = "bundles_active";

    public CreateProductBundleCommandHandler(
        Merge.Application.Interfaces.IRepository<ProductBundle> bundleRepository,
        Merge.Application.Interfaces.IRepository<BundleItem> bundleItemRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<CreateProductBundleCommandHandler> logger)
    {
        _bundleRepository = bundleRepository;
        _bundleItemRepository = bundleItemRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductBundleDto> Handle(CreateProductBundleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product bundle. Name: {Name}, ProductCount: {ProductCount}",
            request.Name, request.Products.Count);

        if (!request.Products.Any())
        {
            throw new ValidationException("Paket en az bir ürün içermelidir.");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ PERFORMANCE: Fetch all products in a single query to avoid N+1
            var productIds = request.Products.Select(p => p.ProductId).ToList();
            var products = await _context.Set<ProductEntity>()
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id) && p.IsActive)
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            // Validate all products exist
            foreach (var productDto in request.Products)
            {
                if (!products.ContainsKey(productDto.ProductId))
                {
                    throw new NotFoundException("Ürün", productDto.ProductId);
                }
            }

            // Calculate original total price
            decimal originalTotal = request.Products.Sum(productDto =>
                (products[productDto.ProductId].DiscountPrice ?? products[productDto.ProductId].Price) * productDto.Quantity);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var bundle = ProductBundle.Create(
                request.Name,
                request.Description,
                request.BundlePrice,
                originalTotal,
                request.ImageUrl,
                request.StartDate,
                request.EndDate);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            foreach (var productDto in request.Products)
            {
                bundle.AddItem(productDto.ProductId, productDto.Quantity, productDto.SortOrder);
            }

            bundle = await _bundleRepository.AddAsync(bundle, cancellationToken);

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with all includes in one query instead of multiple LoadAsync calls (N+1 fix)
            // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (ThenInclude)
            var reloadedBundle = await _context.Set<ProductBundle>()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(b => b.BundleItems)
                    .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(b => b.Id == bundle.Id, cancellationToken);

            if (reloadedBundle == null)
            {
                _logger.LogWarning("Product bundle {BundleId} not found after creation", bundle.Id);
                throw new NotFoundException("Paket", bundle.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_BUNDLES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_BUNDLES, cancellationToken);

            _logger.LogInformation("Product bundle created successfully. BundleId: {BundleId}, Name: {Name}, BundlePrice: {BundlePrice}",
                bundle.Id, request.Name, request.BundlePrice);

            return _mapper.Map<ProductBundleDto>(reloadedBundle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product bundle. Name: {Name}", request.Name);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
