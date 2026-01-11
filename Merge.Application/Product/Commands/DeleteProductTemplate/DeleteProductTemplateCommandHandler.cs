using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;

namespace Merge.Application.Product.Commands.DeleteProductTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteProductTemplateCommandHandler : IRequestHandler<DeleteProductTemplateCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductTemplateCommandHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private const string CACHE_KEY_TEMPLATE_BY_ID = "product_template_";
    private const string CACHE_KEY_ALL_TEMPLATES = "product_templates_all";
    private const string CACHE_KEY_TEMPLATES_BY_CATEGORY = "product_templates_by_category_";
    private const string CACHE_KEY_TEMPLATES_ACTIVE = "product_templates_active";
    private const string CACHE_KEY_POPULAR_TEMPLATES = "product_templates_popular_";

    public DeleteProductTemplateCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteProductTemplateCommandHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<bool> Handle(DeleteProductTemplateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting product template. TemplateId: {TemplateId}", request.Id);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var template = await _context.Set<ProductTemplate>()
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (template == null)
            {
                return false;
            }

            // Store category ID for cache invalidation
            var categoryId = template.CategoryId;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            template.MarkAsDeleted();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_TEMPLATE_BY_ID}{request.Id}", cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ALL_TEMPLATES, cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_TEMPLATES_BY_CATEGORY}{categoryId}_", cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_TEMPLATES_ACTIVE, cancellationToken);
            // Invalidate popular templates cache (all possible limits)
            // ✅ BOLUM 12.0: Magic number YASAK - Config kullan (ZORUNLU)
            for (int limit = _paginationSettings.DefaultPageSize; limit <= _paginationSettings.MaxPageSize; limit += _paginationSettings.DefaultPageSize)
            {
                await _cache.RemoveAsync($"{CACHE_KEY_POPULAR_TEMPLATES}{limit}", cancellationToken);
            }

            _logger.LogInformation("Product template deleted successfully. TemplateId: {TemplateId}", request.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product template. TemplateId: {TemplateId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
