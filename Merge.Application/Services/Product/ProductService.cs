using AutoMapper;
using UserEntity = Merge.Domain.Entities.User;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Product;
using Merge.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Merge.Application.Services.Product;

public class ProductService : IProductService
{
    private readonly IRepository<ProductEntity> _productRepository;
    private readonly IDbContext _context; // ✅ BOLUM 1.0: IDbContext kullan (Clean Architecture)
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;
    private readonly PaginationSettings _paginationSettings;

    public ProductService(
        IRepository<ProductEntity> productRepository,
        IDbContext context, // ✅ BOLUM 1.0: IDbContext kullan (Clean Architecture)
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProductService> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _productRepository = productRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed !p.IsDeleted (Global Query Filter handles it)
        // ✅ BOLUM 1.0: IDbContext.Set<T>() kullan (Clean Architecture)
        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<ProductDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 12.0: Magic number YASAK - Config kullan (ZORUNLU)
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + removed manual !p.IsDeleted check
        var query = _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IEnumerable<ProductDto>>(products);

        return new PagedResult<ProductDto>
        {
            Items = dtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<ProductDto>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 12.0: Magic number YASAK - Config kullan (ZORUNLU)
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + removed manual !p.IsDeleted check
        var query = _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.CategoryId == categoryId);

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Retrieved products by category. CategoryId: {CategoryId}, Count: {Count}, TotalCount: {TotalCount}",
            categoryId, products.Count, totalCount);

        var dtos = _mapper.Map<IEnumerable<ProductDto>>(products);

        return new PagedResult<ProductDto>
        {
            Items = dtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<ProductDto>> SearchAsync(string searchTerm, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            throw new ValidationException("Arama terimi boş olamaz.");
        }

        // ✅ BOLUM 12.0: Magic number YASAK - Config kullan (ZORUNLU)
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: EF.Functions.ILike for case-insensitive search with PostgreSQL
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed !p.IsDeleted (Global Query Filter)
        var query = _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive &&
                (EF.Functions.ILike(p.Name, $"%{searchTerm}%") ||
                 EF.Functions.ILike(p.Description, $"%{searchTerm}%") ||
                 EF.Functions.ILike(p.Brand, $"%{searchTerm}%")));

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product search completed. Term: {SearchTerm}, Results: {Count}, TotalCount: {TotalCount}",
            searchTerm, products.Count, totalCount);

        var dtos = _mapper.Map<IEnumerable<ProductDto>>(products);

        return new PagedResult<ProductDto>
        {
            Items = dtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<ProductDto> CreateAsync(ProductDto productDto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product oluşturuluyor. Name: {Name}, SKU: {SKU}, SellerId: {SellerId}",
            productDto.Name, productDto.SKU, productDto.SellerId);

        if (productDto == null)
        {
            throw new ArgumentNullException(nameof(productDto));
        }

        if (string.IsNullOrWhiteSpace(productDto.Name))
        {
            throw new ValidationException("Ürün adı boş olamaz.");
        }

        if (productDto.Price < 0)
        {
            throw new ValidationException("Ürün fiyatı negatif olamaz.");
        }

        var product = _mapper.Map<ProductEntity>(productDto);
        product = await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        // ✅ BOLUM 1.0: IDbContext.Set<T>() kullan (Clean Architecture)
        product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product oluşturuldu. ProductId: {ProductId}, Name: {Name}, SKU: {SKU}",
            product!.Id, productDto.Name, productDto.SKU);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, ProductDto productDto, CancellationToken cancellationToken = default)
    {
        if (productDto == null)
        {
            throw new ArgumentNullException(nameof(productDto));
        }

        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            throw new NotFoundException("Ürün", id);
        }

        if (string.IsNullOrWhiteSpace(productDto.Name))
        {
            throw new ValidationException("Ürün adı boş olamaz.");
        }

        if (productDto.Price < 0)
        {
            throw new ValidationException("Ürün fiyatı negatif olamaz.");
        }

        if (productDto.StockQuantity < 0)
        {
            throw new ValidationException("Stok miktarı negatif olamaz.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        product.UpdateName(productDto.Name);
        product.UpdateDescription(productDto.Description);
        
        var price = new Money(productDto.Price);
        product.SetPrice(price);
        
        if (productDto.DiscountPrice.HasValue)
        {
            var discountPrice = new Money(productDto.DiscountPrice.Value);
            product.SetDiscountPrice(discountPrice);
        }
        else
        {
            product.SetDiscountPrice(null);
        }
        
        product.SetStockQuantity(productDto.StockQuantity);
        product.UpdateBrand(productDto.Brand);
        product.UpdateImages(productDto.ImageUrl, productDto.ImageUrls?.ToList() ?? new List<string>());
        
        if (productDto.IsActive)
            product.Activate();
        else
            product.Deactivate();
        
        product.SetCategory(productDto.CategoryId);

        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        // ✅ BOLUM 1.0: IDbContext.Set<T>() kullan (Clean Architecture)
        product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return _mapper.Map<ProductDto>(product);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return false;
        }

        await _productRepository.DeleteAsync(product);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

