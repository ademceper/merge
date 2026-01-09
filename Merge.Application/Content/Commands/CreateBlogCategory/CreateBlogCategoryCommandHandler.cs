using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Content.Commands.CreateBlogCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateBlogCategoryCommandHandler : IRequestHandler<CreateBlogCategoryCommand, BlogCategoryDto>
{
    private readonly IRepository<BlogCategory> _categoryRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBlogCategoryCommandHandler> _logger;
    private const string CACHE_KEY_ALL_CATEGORIES = "blog_categories_all";
    private const string CACHE_KEY_ACTIVE_CATEGORIES = "blog_categories_active";

    public CreateBlogCategoryCommandHandler(
        IRepository<BlogCategory> categoryRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<CreateBlogCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BlogCategoryDto> Handle(CreateBlogCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating blog category. Name: {Name}", request.Name);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Slug uniqueness check
            var slug = BlogCategory.GenerateSlug(request.Name);
            if (await _context.Set<BlogCategory>().AnyAsync(c => c.Slug == slug, cancellationToken))
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks}"; // Append timestamp for uniqueness
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var category = BlogCategory.Create(
                request.Name,
                request.Description,
                request.ParentCategoryId,
                request.ImageUrl,
                request.DisplayOrder,
                request.IsActive,
                slug); // Pass unique slug

            category = await _categoryRepository.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            var reloadedCategory = await _context.Set<BlogCategory>()
                .AsNoTracking()
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken);

            if (reloadedCategory == null)
            {
                _logger.LogWarning("Blog category {CategoryId} not found after creation", category.Id);
                throw new NotFoundException("Blog Kategorisi", category.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_CATEGORIES, cancellationToken);
            await _cache.RemoveAsync($"blog_category_{category.Id}", cancellationToken); // Single category cache
            await _cache.RemoveAsync($"blog_category_slug_{category.Slug}", cancellationToken); // Slug cache

            _logger.LogInformation("Blog category created. CategoryId: {CategoryId}, Name: {Name}", category.Id, category.Name);

            return _mapper.Map<BlogCategoryDto>(reloadedCategory);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while creating blog category with Name: {Name}", request.Name);
            throw new BusinessException("Blog kategorisi oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error creating blog category with Name: {Name}", request.Name);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

