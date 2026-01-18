using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Catalog;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.Category>;

namespace Merge.Application.Catalog.Commands.PatchCategory;

/// <summary>
/// Handler for PatchCategoryCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchCategoryCommandHandler(
    IRepository categoryRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<PatchCategoryCommandHandler> logger) : IRequestHandler<PatchCategoryCommand, CategoryDto>
{
    private const string CACHE_KEY_ALL_CATEGORIES = "categories_all";
    private const string CACHE_KEY_MAIN_CATEGORIES = "categories_main";

    public async Task<CategoryDto> Handle(PatchCategoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching category with Id: {CategoryId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category is null)
            {
                throw new NotFoundException("Kategori", request.Id);
            }

            // Apply partial updates - only update fields that are provided
            if (request.PatchDto.Name is not null)
            {
                category.UpdateName(request.PatchDto.Name);
            }

            if (request.PatchDto.Description is not null)
            {
                category.UpdateDescription(request.PatchDto.Description);
            }

            if (request.PatchDto.Slug is not null)
            {
                var slug = new Slug(request.PatchDto.Slug);
                category.UpdateSlug(slug);
            }

            if (request.PatchDto.ImageUrl is not null)
            {
                category.UpdateImageUrl(request.PatchDto.ImageUrl);
            }

            if (request.PatchDto.ParentCategoryId.HasValue)
            {
                category.SetParentCategory(request.PatchDto.ParentCategoryId);
            }
            else if (request.PatchDto.ParentCategoryId is null && category.ParentCategoryId.HasValue)
            {
                // Explicit null means remove parent
                category.SetParentCategory(null);
            }

            await categoryRepository.UpdateAsync(category, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // Reload with includes
            var reloadedCategory = await context.Set<Category>()
                .AsNoTracking()
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (reloadedCategory is null)
            {
                logger.LogWarning("Category {CategoryId} not found after patch", request.Id);
                throw new NotFoundException("Kategori", request.Id);
            }

            // Cache invalidation
            await cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_MAIN_CATEGORIES, cancellationToken);
            await cache.RemoveAsync($"category_{request.Id}", cancellationToken);

            logger.LogInformation("Category patched successfully with Id: {CategoryId}", request.Id);

            return mapper.Map<CategoryDto>(reloadedCategory);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error patching category Id: {CategoryId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
