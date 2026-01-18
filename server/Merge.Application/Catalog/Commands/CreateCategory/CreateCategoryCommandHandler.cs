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
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.Category>;

namespace Merge.Application.Catalog.Commands.CreateCategory;

public class CreateCategoryCommandHandler(
    IRepository categoryRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<CreateCategoryCommandHandler> logger) : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private const string CACHE_KEY_ALL_CATEGORIES = "categories_all";
    private const string CACHE_KEY_MAIN_CATEGORIES = "categories_main";
    private const string CACHE_KEY_ALL_CATEGORIES_PAGED = "categories_all_paged";
    private const string CACHE_KEY_MAIN_CATEGORIES_PAGED = "categories_main_paged";

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating category with Name: {Name}, Slug: {Slug}", request.Name, request.Slug);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var slug = new Slug(request.Slug);
            var category = Category.Create(
                request.Name,
                request.Description,
                slug,
                request.ImageUrl,
                request.ParentCategoryId);

            category = await categoryRepository.AddAsync(category, cancellationToken);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedCategory = await context.Set<Category>()
                .AsNoTracking()
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken);

            if (reloadedCategory is null)
            {
                logger.LogWarning("Category {CategoryId} not found after creation", category.Id);
                throw new NotFoundException("Kategori", category.Id);
            }

            await cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_MAIN_CATEGORIES, cancellationToken);
            await cache.RemoveAsync($"category_{category.Id}", cancellationToken); // Single category cache
            // Note: Paginated cache'ler (categories_all_paged_*, categories_main_paged_*) 
            // pattern-based invalidation gerektirir. ICacheService'de RemoveByPrefixAsync yok.
            // Şimdilik cache expiration'a güveniyoruz (1 saat TTL)

            logger.LogInformation("Category created successfully with Id: {CategoryId}", category.Id);

            return mapper.Map<CategoryDto>(reloadedCategory);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while creating category with Name: {Name}, Slug: {Slug}", request.Name, request.Slug);
            throw new BusinessException("Kategori oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating category with Name: {Name}, Slug: {Slug}", request.Name, request.Slug);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
