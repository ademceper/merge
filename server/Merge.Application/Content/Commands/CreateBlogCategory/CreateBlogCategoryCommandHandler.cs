using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.BlogCategory>;

namespace Merge.Application.Content.Commands.CreateBlogCategory;

public class CreateBlogCategoryCommandHandler(
    IRepository categoryRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<CreateBlogCategoryCommandHandler> logger) : IRequestHandler<CreateBlogCategoryCommand, BlogCategoryDto>
{
    private const string CACHE_KEY_ALL_CATEGORIES = "blog_categories_all";
    private const string CACHE_KEY_ACTIVE_CATEGORIES = "blog_categories_active";

    public async Task<BlogCategoryDto> Handle(CreateBlogCategoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating blog category. Name: {Name}", request.Name);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var slugValue = Slug.FromString(request.Name).Value;
            if (await context.Set<BlogCategory>().AnyAsync(c => c.Slug.Value == slugValue, cancellationToken))
            {
                slugValue = $"{slugValue}-{DateTime.UtcNow.Ticks}";
            }

            var category = BlogCategory.Create(
                request.Name,
                request.Description,
                request.ParentCategoryId,
                request.ImageUrl,
                request.DisplayOrder,
                request.IsActive,
                slugValue);

            category = await categoryRepository.AddAsync(category, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedCategory = await context.Set<BlogCategory>()
                .AsNoTracking()
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken);

            if (reloadedCategory is null)
            {
                logger.LogWarning("Blog category {CategoryId} not found after creation", category.Id);
                throw new NotFoundException("Blog Kategorisi", category.Id);
            }

            await cache.RemoveAsync(CACHE_KEY_ALL_CATEGORIES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_CATEGORIES, cancellationToken);
            await cache.RemoveAsync($"blog_category_{category.Id}", cancellationToken);
            await cache.RemoveAsync($"blog_category_slug_{category.Slug}", cancellationToken);

            logger.LogInformation("Blog category created. CategoryId: {CategoryId}, Name: {Name}", category.Id, category.Name);

            return mapper.Map<BlogCategoryDto>(reloadedCategory);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while creating blog category with Name: {Name}", request.Name);
            throw new BusinessException("Blog kategorisi oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating blog category with Name: {Name}", request.Name);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

