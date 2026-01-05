using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Catalog;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Catalog;
using Merge.Application.Common;


namespace Merge.Application.Services.Catalog;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IDbContext _context; // ✅ BOLUM 1.0: IDbContext kullan (Clean Architecture)
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache; // ✅ BOLUM 10.2: Redis distributed cache
    private readonly ILogger<CategoryService> _logger;
    private const string CACHE_KEY_ALL_CATEGORIES = "categories_all";
    private const string CACHE_KEY_MAIN_CATEGORIES = "categories_main";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromHours(1);

    public CategoryService(
        IRepository<Category> categoryRepository,
        IDbContext context, // ✅ BOLUM 1.0: IDbContext kullan (Clean Architecture)
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cache, // ✅ BOLUM 10.2: Redis distributed cache
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted and !c.IsDeleted (Global Query Filter)
        var category = await _context.Set<Category>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return category == null ? null : _mapper.Map<CategoryDto>(category);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 10.2: Redis distributed cache for frequently accessed, rarely changed data
        // 10,000 requests/day → Only ~10 DB queries (1 per hour)
        // 1000x reduction in database load
        var categories = await _cache.GetOrCreateAsync(
            CACHE_KEY_ALL_CATEGORIES,
            async () =>
            {
                _logger.LogInformation("Cache miss for all categories. Fetching from database.");

                // ✅ PERFORMANCE: Removed !c.IsDeleted check (Global Query Filter)
                var categoryList = await _context.Set<Category>()
                    .AsNoTracking()
                    .Include(c => c.ParentCategory)
                    .OrderBy(c => c.Name)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Cached {Count} categories", categoryList.Count);

                return _mapper.Map<List<CategoryDto>>(categoryList);
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return categories ?? Enumerable.Empty<CategoryDto>();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PagedResult<CategoryDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Category>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var categories = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} categories (page {Page})", categories.Count, page);

        return new PagedResult<CategoryDto>
        {
            Items = _mapper.Map<List<CategoryDto>>(categories),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<CategoryDto>> GetMainCategoriesAsync(CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 10.2: Redis distributed cache for main categories
        var categories = await _cache.GetOrCreateAsync(
            CACHE_KEY_MAIN_CATEGORIES,
            async () =>
            {
                _logger.LogInformation("Cache miss for main categories. Fetching from database.");

                // ✅ PERFORMANCE: Removed manual !sc.IsDeleted and !c.IsDeleted checks
                var categoryList = await _context.Set<Category>()
                    .AsNoTracking()
                    .Include(c => c.SubCategories)
                    .Where(c => c.ParentCategoryId == null)
                    .OrderBy(c => c.Name)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Cached {Count} main categories", categoryList.Count);

                return _mapper.Map<List<CategoryDto>>(categoryList);
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return categories ?? Enumerable.Empty<CategoryDto>();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PagedResult<CategoryDto>> GetMainCategoriesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Category>()
            .AsNoTracking()
            .Include(c => c.SubCategories)
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var categories = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} main categories (page {Page})", categories.Count, page);

        return new PagedResult<CategoryDto>
        {
            Items = _mapper.Map<List<CategoryDto>>(categories),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CategoryDto> CreateAsync(CategoryDto categoryDto, CancellationToken cancellationToken = default)
    {
        if (categoryDto == null)
        {
            throw new ArgumentNullException(nameof(categoryDto));
        }

        if (string.IsNullOrWhiteSpace(categoryDto.Name))
        {
            throw new ValidationException("Kategori adı boş olamaz.");
        }

        // ✅ BOLUM 1.1: Factory Method kullanımı
        var category = Category.Create(
            categoryDto.Name,
            categoryDto.Description,
            categoryDto.Slug,
            categoryDto.ImageUrl,
            categoryDto.ParentCategoryId);
        
        category = await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Performance: Invalidate cache when data changes
        await _cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
        await _cache.RemoveAsync(CACHE_KEY_MAIN_CATEGORIES, cancellationToken);
        
        return _mapper.Map<CategoryDto>(category);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CategoryDto> UpdateAsync(Guid id, CategoryDto categoryDto, CancellationToken cancellationToken = default)
    {
        if (categoryDto == null)
        {
            throw new ArgumentNullException(nameof(categoryDto));
        }

        if (string.IsNullOrWhiteSpace(categoryDto.Name))
        {
            throw new ValidationException("Kategori adı boş olamaz.");
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException("Kategori", id);
        }

        // ✅ BOLUM 1.1: Domain Method kullanımı
        category.UpdateName(categoryDto.Name);
        category.UpdateDescription(categoryDto.Description);
        category.UpdateSlug(categoryDto.Slug);
        category.UpdateImageUrl(categoryDto.ImageUrl);
        category.SetParentCategory(categoryDto.ParentCategoryId);

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Performance: Invalidate cache when data changes
        await _cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
        await _cache.RemoveAsync(CACHE_KEY_MAIN_CATEGORIES, cancellationToken);
        
        return _mapper.Map<CategoryDto>(category);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null)
        {
            return false;
        }

        await _categoryRepository.DeleteAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Performance: Invalidate cache when data changes
        await _cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
        await _cache.RemoveAsync(CACHE_KEY_MAIN_CATEGORIES, cancellationToken);
        
        return true;
    }
}

