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

namespace Merge.Application.Content.Commands.UpdateBlogCategory;

public class UpdateBlogCategoryCommandHandler(
    Merge.Application.Interfaces.IRepository<BlogCategory> categoryRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<UpdateBlogCategoryCommandHandler> logger) : IRequestHandler<UpdateBlogCategoryCommand, bool>
{
    private const string CACHE_KEY_ALL_CATEGORIES = "blog_categories_all";
    private const string CACHE_KEY_ACTIVE_CATEGORIES = "blog_categories_active";

    public async Task<bool> Handle(UpdateBlogCategoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating blog category. CategoryId: {CategoryId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                logger.LogWarning("Blog category not found. CategoryId: {CategoryId}", request.Id);
                return false;
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                category.UpdateName(request.Name);
            }
            if (request.Description != null)
                category.UpdateDescription(request.Description);
            if (request.ParentCategoryId.HasValue)
                category.UpdateParentCategory(request.ParentCategoryId);
            else if (request.ParentCategoryId == null && category.ParentCategoryId.HasValue)
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

            await categoryRepository.UpdateAsync(category, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_CATEGORIES, cancellationToken);
            await cache.RemoveAsync($"blog_category_{request.Id}", cancellationToken);
            await cache.RemoveAsync($"blog_category_slug_{category.Slug}", cancellationToken);

            logger.LogInformation("Blog category updated. CategoryId: {CategoryId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while updating blog category Id: {CategoryId}", request.Id);
            throw new BusinessException("Blog kategorisi güncelleme çakışması. Başka bir kullanıcı aynı kategoriyi güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating blog category Id: {CategoryId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

