using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.UpdateProductTemplate;

public class UpdateProductTemplateCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<UpdateProductTemplateCommandHandler> logger, ICacheService cache, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<UpdateProductTemplateCommand, bool>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    private const string CACHE_KEY_TEMPLATE_BY_ID = "product_template_";
    private const string CACHE_KEY_ALL_TEMPLATES = "product_templates_all";
    private const string CACHE_KEY_TEMPLATES_BY_CATEGORY = "product_templates_by_category_";
    private const string CACHE_KEY_TEMPLATES_ACTIVE = "product_templates_active";
    private const string CACHE_KEY_POPULAR_TEMPLATES = "product_templates_popular_";

    public async Task<bool> Handle(UpdateProductTemplateCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating product template. TemplateId: {TemplateId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var template = await context.Set<ProductTemplate>()
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (template == null)
            {
                return false;
            }

            // Store old category ID for cache invalidation
            var oldCategoryId = template.CategoryId;

            template.Update(
                request.Name,
                request.Description,
                request.CategoryId ?? template.CategoryId,
                request.Brand,
                request.DefaultSKUPrefix,
                request.DefaultPrice,
                request.DefaultStockQuantity,
                request.DefaultImageUrl,
                request.Specifications != null ? JsonSerializer.Serialize(request.Specifications) : null,
                request.Attributes != null ? JsonSerializer.Serialize(request.Attributes) : null,
                request.IsActive ?? template.IsActive);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_TEMPLATE_BY_ID}{request.Id}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_TEMPLATES, cancellationToken);
            if (request.CategoryId.HasValue && request.CategoryId.Value != oldCategoryId)
            {
                await cache.RemoveAsync($"{CACHE_KEY_TEMPLATES_BY_CATEGORY}{oldCategoryId}_", cancellationToken);
                await cache.RemoveAsync($"{CACHE_KEY_TEMPLATES_BY_CATEGORY}{request.CategoryId.Value}_", cancellationToken);
            }
            else
            {
                await cache.RemoveAsync($"{CACHE_KEY_TEMPLATES_BY_CATEGORY}{oldCategoryId}_", cancellationToken);
            }
            await cache.RemoveAsync(CACHE_KEY_TEMPLATES_ACTIVE, cancellationToken);
            // Invalidate popular templates cache (all possible limits)
            for (int limit = paginationConfig.DefaultPageSize; limit <= paginationConfig.MaxPageSize; limit += paginationConfig.DefaultPageSize)
            {
                await cache.RemoveAsync($"{CACHE_KEY_POPULAR_TEMPLATES}{limit}", cancellationToken);
            }

            logger.LogInformation("Product template updated successfully. TemplateId: {TemplateId}", request.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product template. TemplateId: {TemplateId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
