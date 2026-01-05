using AutoMapper;
using CartEntity = Merge.Domain.Entities.Cart;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.Cart;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Microsoft.Extensions.Logging;


namespace Merge.Application.Services.Cart;

public class WishlistService : IWishlistService
{
    private readonly IRepository<Wishlist> _wishlistRepository;
    private readonly IRepository<ProductEntity> _productRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<WishlistService> _logger;

    public WishlistService(
        IRepository<Wishlist> wishlistRepository,
        IRepository<ProductEntity> productRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<WishlistService> logger)
    {
        _wishlistRepository = wishlistRepository;
        _productRepository = productRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductDto>> GetWishlistAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving wishlist for user {UserId}", userId);

        // ✅ PERFORMANCE FIX: AsNoTracking for read-only queries
        // ✅ PERFORMANCE FIX: Removed manual !w.IsDeleted check (Global Query Filter handles it)
        // ✅ PERFORMANCE FIX: Removed manual !p.IsDeleted check (Global Query Filter handles it)
        var wishlistItems = await _context.Set<Wishlist>()
            .AsNoTracking()
            .Include(w => w.Product)
                .ThenInclude(p => p.Category)
            .Where(w => w.UserId == userId)
            .Select(w => w.Product)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} items from wishlist for user {UserId}",
            wishlistItems.Count, userId);

        return _mapper.Map<IEnumerable<ProductDto>>(wishlistItems);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PagedResult<ProductDto>> GetWishlistAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving wishlist (page {Page}) for user {UserId}", page, userId);

        var query = _context.Set<Wishlist>()
            .AsNoTracking()
            .Include(w => w.Product)
                .ThenInclude(p => p.Category)
            .Where(w => w.UserId == userId)
            .Select(w => w.Product)
            .Where(p => p.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);
        var wishlistItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} items (page {Page}) from wishlist for user {UserId}",
            wishlistItems.Count, page, userId);

        return new PagedResult<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(wishlistItems),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> AddToWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding product {ProductId} to wishlist for user {UserId}",
            productId, userId);

        // ✅ PERFORMANCE FIX: AsNoTracking for read-only check
        // ✅ PERFORMANCE FIX: Removed manual !w.IsDeleted check (Global Query Filter handles it)
        var existing = await _context.Set<Wishlist>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);

        if (existing != null)
        {
            _logger.LogWarning(
                "Product {ProductId} already exists in wishlist for user {UserId}",
                productId, userId);
            return false; // Zaten favorilerde
        }

        // ✅ PERFORMANCE: AsNoTracking for read-only product query
        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null || !product.IsActive)
        {
            _logger.LogWarning(
                "Product {ProductId} not found or inactive for user {UserId}",
                productId, userId);
            throw new NotFoundException("Ürün", productId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
        var wishlist = Wishlist.Create(userId, productId);

        await _wishlistRepository.AddAsync(wishlist);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // ✅ CRITICAL FIX: Explicit SaveChanges via UnitOfWork

        _logger.LogInformation("Successfully added product {ProductId} to wishlist for user {UserId}",
            productId, userId);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> RemoveFromWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing product {ProductId} from wishlist for user {UserId}",
            productId, userId);

        // ✅ PERFORMANCE FIX: Removed manual !w.IsDeleted check (Global Query Filter handles it)
        var wishlist = await _context.Set<Wishlist>()
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);

        if (wishlist == null)
        {
            _logger.LogWarning(
                "Wishlist item not found for product {ProductId} and user {UserId}",
                productId, userId);
            return false;
        }

        await _wishlistRepository.DeleteAsync(wishlist);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // ✅ CRITICAL FIX: Explicit SaveChanges via UnitOfWork

        _logger.LogInformation("Successfully removed product {ProductId} from wishlist for user {UserId}",
            productId, userId);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> IsInWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking if product {ProductId} is in wishlist for user {UserId}",
            productId, userId);

        // ✅ PERFORMANCE FIX: AsNoTracking for read-only queries
        // ✅ PERFORMANCE FIX: Removed manual !w.IsDeleted check (Global Query Filter handles it)
        var exists = await _context.Set<Wishlist>()
            .AsNoTracking()
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);

        _logger.LogDebug("Product {ProductId} exists in wishlist for user {UserId}: {Exists}",
            productId, userId, exists);

        return exists;
    }
}

