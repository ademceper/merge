using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.DeleteProductTemplate;

public class DeleteProductTemplateCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteProductTemplateCommandHandler> logger, ICacheService cache, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<DeleteProductTemplateCommand, bool>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    private const string CACHE_KEY_TEMPLATE_BY_ID = "product_template_";
    private const string CACHE_KEY_ALL_TEMPLATES = "product_templates_all";
    private const string CACHE_KEY_TEMPLATES_BY_CATEGORY = "product_templates_by_category_";
    private const string CACHE_KEY_TEMPLATES_ACTIVE = "product_templates_active";
    private const string CACHE_KEY_POPULAR_TEMPLATES = "product_templates_popular_";

    public async Task<bool> Handle(DeleteProductTemplateCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting product template. TemplateId: {TemplateId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var template = await context.Set<ProductTemplate>()
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (template is null)
            {
                return false;
            }

            // Store category ID for cache invalidation
            var categoryId = template.CategoryId;

            template.MarkAsDeleted();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_TEMPLATE_BY_ID}{request.Id}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_TEMPLATES, cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_TEMPLATES_BY_CATEGORY}{categoryId}_", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_TEMPLATES_ACTIVE, cancellationToken);
            // Invalidate popular templates cache (all possible limits)
            for (int limit = paginationConfig.DefaultPageSize; limit <= paginationConfig.MaxPageSize; limit += paginationConfig.DefaultPageSize)
            {
                await cache.RemoveAsync($"{CACHE_KEY_POPULAR_TEMPLATES}{limit}", cancellationToken);
            }

            logger.LogInformation("Product template deleted successfully. TemplateId: {TemplateId}", request.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting product template. TemplateId: {TemplateId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
