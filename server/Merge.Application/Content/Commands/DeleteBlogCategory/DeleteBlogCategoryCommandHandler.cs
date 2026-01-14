using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Commands.DeleteBlogCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteBlogCategoryCommandHandler : IRequestHandler<DeleteBlogCategoryCommand, bool>
{
    private readonly Merge.Application.Interfaces.IRepository<BlogCategory> _categoryRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<DeleteBlogCategoryCommandHandler> _logger;
    private const string CACHE_KEY_ALL_CATEGORIES = "blog_categories_all";
    private const string CACHE_KEY_ACTIVE_CATEGORIES = "blog_categories_active";

    public DeleteBlogCategoryCommandHandler(
        Merge.Application.Interfaces.IRepository<BlogCategory> categoryRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<DeleteBlogCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteBlogCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting blog category. CategoryId: {CategoryId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                _logger.LogWarning("Blog category not found for deletion. CategoryId: {CategoryId}", request.Id);
                return false;
            }

            // Check for subcategories
            var hasSubCategories = await _context.Set<BlogCategory>()
                .AnyAsync(c => c.ParentCategoryId == request.Id, cancellationToken);
            if (hasSubCategories)
            {
                throw new BusinessException("Bu kategorinin alt kategorileri bulunmaktadır. Lütfen önce alt kategorileri silin.");
            }

            // Check for posts
            var hasPosts = await _context.Set<BlogPost>()
                .AnyAsync(p => p.CategoryId == request.Id, cancellationToken);
            if (hasPosts)
            {
                throw new BusinessException("Bu kategoride blog yazıları bulunmaktadır. Lütfen önce blog yazılarını silin veya başka bir kategoriye taşıyın.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (Soft Delete)
            category.MarkAsDeleted();

            await _categoryRepository.UpdateAsync(category, cancellationToken); // Soft delete olduğu için Update
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_CATEGORIES, cancellationToken);
            await _cache.RemoveAsync($"blog_category_{request.Id}", cancellationToken); // Single category cache
            await _cache.RemoveAsync($"blog_category_slug_{category.Slug}", cancellationToken); // Slug cache

            _logger.LogInformation("Blog category deleted (soft delete). CategoryId: {CategoryId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while deleting blog category Id: {CategoryId}", request.Id);
            throw new BusinessException("Blog kategorisi silme çakışması. Başka bir kullanıcı aynı kategoriyi güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error deleting blog category Id: {CategoryId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

