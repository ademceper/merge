using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Catalog;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Catalog.Commands.UpdateCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateCategoryCommandHandler(
    Merge.Application.Interfaces.IRepository<Category> categoryRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<UpdateCategoryCommandHandler> logger) : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private const string CACHE_KEY_ALL_CATEGORIES = "categories_all";
    private const string CACHE_KEY_MAIN_CATEGORIES = "categories_main";
    private const string CACHE_KEY_ALL_CATEGORIES_PAGED = "categories_all_paged";
    private const string CACHE_KEY_MAIN_CATEGORIES_PAGED = "categories_main_paged";

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating category with Id: {CategoryId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                throw new NotFoundException("Kategori", request.Id);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            category.UpdateName(request.Name);
            category.UpdateDescription(request.Description);
            // ✅ BOLUM 1.3: Value Objects - Slug Value Object kullanımı
            if (!string.IsNullOrEmpty(request.Slug))
            {
                var slug = new Slug(request.Slug);
                category.UpdateSlug(slug);
            }
            category.UpdateImageUrl(request.ImageUrl);
            category.SetParentCategory(request.ParentCategoryId);

            await categoryRepository.UpdateAsync(category, cancellationToken);
            
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            var reloadedCategory = await context.Set<Category>()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (reloadedCategory == null)
            {
                logger.LogWarning("Category {CategoryId} not found after update", request.Id);
                throw new NotFoundException("Kategori", request.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation - Remove all category-related cache
            await cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_MAIN_CATEGORIES, cancellationToken);
            await cache.RemoveAsync($"category_{request.Id}", cancellationToken); // Single category cache
            // Note: Paginated cache'ler (categories_all_paged_*, categories_main_paged_*) 
            // pattern-based invalidation gerektirir. ICacheService'de RemoveByPrefixAsync yok.
            // Şimdilik cache expiration'a güveniyoruz (1 saat TTL)

            logger.LogInformation("Category updated successfully with Id: {CategoryId}", request.Id);

            return mapper.Map<CategoryDto>(reloadedCategory);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while updating category Id: {CategoryId}", request.Id);
            throw new BusinessException("Kategori güncelleme çakışması. Başka bir kullanıcı aynı kategoriyi güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex, "Error updating category Id: {CategoryId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
