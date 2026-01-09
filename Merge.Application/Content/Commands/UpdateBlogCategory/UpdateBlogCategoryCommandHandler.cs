using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Commands.UpdateBlogCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateBlogCategoryCommandHandler : IRequestHandler<UpdateBlogCategoryCommand, bool>
{
    private readonly IRepository<BlogCategory> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<UpdateBlogCategoryCommandHandler> _logger;
    private const string CACHE_KEY_ALL_CATEGORIES = "blog_categories_all";
    private const string CACHE_KEY_ACTIVE_CATEGORIES = "blog_categories_active";

    public UpdateBlogCategoryCommandHandler(
        IRepository<BlogCategory> categoryRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<UpdateBlogCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateBlogCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating blog category. CategoryId: {CategoryId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                _logger.LogWarning("Blog category not found. CategoryId: {CategoryId}", request.Id);
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            if (!string.IsNullOrEmpty(request.Name))
            {
                category.UpdateName(request.Name);
            }
            if (request.Description != null)
                category.UpdateDescription(request.Description);
            if (request.ParentCategoryId.HasValue)
                category.UpdateParentCategory(request.ParentCategoryId);
            else if (request.ParentCategoryId == null && category.ParentCategoryId.HasValue) // Clear parent
                category.UpdateParentCategory(null);
            if (request.ImageUrl != null)
                category.UpdateImageUrl(request.ImageUrl);
            if (request.DisplayOrder.HasValue)
                category.UpdateDisplayOrder(request.DisplayOrder.Value);
            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                    category.Activate();
                else
                    category.Deactivate();
            }

            await _categoryRepository.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_CATEGORIES, cancellationToken);
            await _cache.RemoveAsync($"blog_category_{request.Id}", cancellationToken); // Single category cache
            await _cache.RemoveAsync($"blog_category_slug_{category.Slug}", cancellationToken); // Slug cache

            _logger.LogInformation("Blog category updated. CategoryId: {CategoryId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while updating blog category Id: {CategoryId}", request.Id);
            throw new BusinessException("Blog kategorisi güncelleme çakışması. Başka bir kullanıcı aynı kategoriyi güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error updating blog category Id: {CategoryId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

