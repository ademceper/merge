using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Catalog;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Catalog;


namespace Merge.Application.Services.Catalog;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CategoryService> _logger;
    private const string CACHE_KEY_ALL_CATEGORIES = "categories_all";
    private const string CACHE_KEY_MAIN_CATEGORIES = "categories_main";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromHours(1);

    public CategoryService(
        IRepository<Category> categoryRepository,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IMemoryCache cache,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted and !c.IsDeleted (Global Query Filter)
        var category = await _context.Categories
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        return category == null ? null : _mapper.Map<CategoryDto>(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        // ✅ PERFORMANCE: Memory cache for frequently accessed, rarely changed data
        // 10,000 requests/day → Only ~10 DB queries (1 per hour)
        // 1000x reduction in database load
        return await _cache.GetOrCreateAsync(CACHE_KEY_ALL_CATEGORIES, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CACHE_EXPIRATION;
            entry.Size = 1;

            _logger.LogInformation("Cache miss for all categories. Fetching from database.");

            // ✅ PERFORMANCE: Removed !c.IsDeleted check (Global Query Filter)
            var categories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.Name)
                .ToListAsync();

            _logger.LogInformation("Cached {Count} categories", categories.Count);

            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }) ?? Enumerable.Empty<CategoryDto>();
    }

    public async Task<IEnumerable<CategoryDto>> GetMainCategoriesAsync()
    {
        // ✅ PERFORMANCE: Memory cache for main categories
        return await _cache.GetOrCreateAsync(CACHE_KEY_MAIN_CATEGORIES, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CACHE_EXPIRATION;
            entry.Size = 1;

            _logger.LogInformation("Cache miss for main categories. Fetching from database.");

            // ✅ PERFORMANCE: Removed manual !sc.IsDeleted and !c.IsDeleted checks
            var categories = await _context.Categories
                .AsNoTracking()
                .Include(c => c.SubCategories)
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();

            _logger.LogInformation("Cached {Count} main categories", categories.Count);

            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }) ?? Enumerable.Empty<CategoryDto>();
    }

    public async Task<CategoryDto> CreateAsync(CategoryDto categoryDto)
    {
        if (categoryDto == null)
        {
            throw new ArgumentNullException(nameof(categoryDto));
        }

        if (string.IsNullOrWhiteSpace(categoryDto.Name))
        {
            throw new ValidationException("Kategori adı boş olamaz.");
        }

        var category = _mapper.Map<Category>(categoryDto);
        category = await _categoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();
        
        // Performance: Invalidate cache when data changes
        _cache.Remove(CACHE_KEY_ALL_CATEGORIES);
        _cache.Remove(CACHE_KEY_MAIN_CATEGORIES);
        
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, CategoryDto categoryDto)
    {
        if (categoryDto == null)
        {
            throw new ArgumentNullException(nameof(categoryDto));
        }

        if (string.IsNullOrWhiteSpace(categoryDto.Name))
        {
            throw new ValidationException("Kategori adı boş olamaz.");
        }

        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            throw new NotFoundException("Kategori", id);
        }

        category.Name = categoryDto.Name;
        category.Description = categoryDto.Description;
        category.Slug = categoryDto.Slug;
        category.ImageUrl = categoryDto.ImageUrl;
        category.ParentCategoryId = categoryDto.ParentCategoryId;

        await _categoryRepository.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();
        
        // Performance: Invalidate cache when data changes
        _cache.Remove(CACHE_KEY_ALL_CATEGORIES);
        _cache.Remove(CACHE_KEY_MAIN_CATEGORIES);
        
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return false;
        }

        await _categoryRepository.DeleteAsync(category);
        await _unitOfWork.SaveChangesAsync();
        
        // Performance: Invalidate cache when data changes
        _cache.Remove(CACHE_KEY_ALL_CATEGORIES);
        _cache.Remove(CACHE_KEY_MAIN_CATEGORIES);
        
        return true;
    }
}

