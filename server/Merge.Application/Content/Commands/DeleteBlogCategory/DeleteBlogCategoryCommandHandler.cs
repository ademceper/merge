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
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.BlogCategory>;

namespace Merge.Application.Content.Commands.DeleteBlogCategory;

public class DeleteBlogCategoryCommandHandler(
    IRepository categoryRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<DeleteBlogCategoryCommandHandler> logger) : IRequestHandler<DeleteBlogCategoryCommand, bool>
{
    private const string CACHE_KEY_ALL_CATEGORIES = "blog_categories_all";
    private const string CACHE_KEY_ACTIVE_CATEGORIES = "blog_categories_active";

    public async Task<bool> Handle(DeleteBlogCategoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting blog category. CategoryId: {CategoryId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                logger.LogWarning("Blog category not found for deletion. CategoryId: {CategoryId}", request.Id);
                return false;
            }

            var hasSubCategories = await context.Set<BlogCategory>()
                .AnyAsync(c => c.ParentCategoryId == request.Id, cancellationToken);
            if (hasSubCategories)
            {
                throw new BusinessException("Bu kategorinin alt kategorileri bulunmaktadır. Lütfen önce alt kategorileri silin.");
            }

            var hasPosts = await context.Set<BlogPost>()
                .AnyAsync(p => p.CategoryId == request.Id, cancellationToken);
            if (hasPosts)
            {
                throw new BusinessException("Bu kategoride blog yazıları bulunmaktadır. Lütfen önce blog yazılarını silin veya başka bir kategoriye taşıyın.");
            }

            category.MarkAsDeleted();

            await categoryRepository.UpdateAsync(category, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_CATEGORIES, cancellationToken);
            await cache.RemoveAsync($"blog_category_{request.Id}", cancellationToken);
            await cache.RemoveAsync($"blog_category_slug_{category.Slug}", cancellationToken);

            logger.LogInformation("Blog category deleted (soft delete). CategoryId: {CategoryId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while deleting blog category Id: {CategoryId}", request.Id);
            throw new BusinessException("Blog kategorisi silme çakışması. Başka bir kullanıcı aynı kategoriyi güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting blog category Id: {CategoryId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

