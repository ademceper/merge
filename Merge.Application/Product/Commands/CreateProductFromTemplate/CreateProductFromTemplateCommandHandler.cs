using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using System.Text.Json;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Product.Commands.CreateProductFromTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateProductFromTemplateCommandHandler : IRequestHandler<CreateProductFromTemplateCommand, ProductDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<CreateProductFromTemplateCommandHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private const string CACHE_KEY_TEMPLATE_BY_ID = "product_template_";
    private const string CACHE_KEY_ALL_TEMPLATES = "product_templates_all";
    private const string CACHE_KEY_TEMPLATES_BY_CATEGORY = "product_templates_by_category_";
    private const string CACHE_KEY_POPULAR_TEMPLATES = "product_templates_popular_";

    public CreateProductFromTemplateCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper,
        ILogger<CreateProductFromTemplateCommandHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<ProductDto> Handle(CreateProductFromTemplateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product from template. TemplateId: {TemplateId}, SellerId: {SellerId}",
            request.TemplateId, request.SellerId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var template = await _context.Set<ProductTemplate>()
                .AsNoTracking()
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.IsActive, cancellationToken);

            if (template == null)
            {
                throw new NotFoundException("Şablon", request.TemplateId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            var sku = new SKU(request.SKU);
            var price = new Money(request.Price);
            var product = ProductEntity.Create(
                request.Name,
                request.Description,
                sku,
                price,
                request.StockQuantity,
                template.CategoryId,
                template.Brand ?? string.Empty,
                request.SellerId,
                request.StoreId
            );

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            if (request.DiscountPrice.HasValue)
            {
                product.SetDiscountPrice(new Money(request.DiscountPrice.Value));
            }

            if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                product.UpdateImages(request.ImageUrl, request.ImageUrls ?? new List<string>());
            }
            else if (!string.IsNullOrEmpty(template.DefaultImageUrl))
            {
                product.UpdateImages(template.DefaultImageUrl, request.ImageUrls ?? new List<string>());
            }
            else if (request.ImageUrls != null && request.ImageUrls.Any())
            {
                // ✅ FIX: CS8625 - UpdateImages non-nullable string bekliyor, ilk imageUrl'i kullan
                product.UpdateImages(request.ImageUrls.First(), request.ImageUrls);
            }

            await _context.Set<ProductEntity>().AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            // Increment template usage count
            var templateToUpdate = await _context.Set<ProductTemplate>()
                .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

            if (templateToUpdate != null)
            {
                templateToUpdate.IncrementUsageCount();
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            product = await _context.Set<ProductEntity>()
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);

            _logger.LogInformation("Product created from template successfully. ProductId: {ProductId}, TemplateId: {TemplateId}",
                product!.Id, request.TemplateId);

            // ✅ BOLUM 10.2: Cache invalidation
            // Invalidate template cache (usage count changed)
            await _cache.RemoveAsync($"{CACHE_KEY_TEMPLATE_BY_ID}{request.TemplateId}", cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ALL_TEMPLATES, cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_TEMPLATES_BY_CATEGORY}{template.CategoryId}_", cancellationToken);
            // Invalidate popular templates cache (all possible limits)
            // ✅ BOLUM 12.0: Magic number YASAK - Config kullan (ZORUNLU)
            for (int limit = _paginationSettings.DefaultPageSize; limit <= _paginationSettings.MaxPageSize; limit += _paginationSettings.DefaultPageSize)
            {
                await _cache.RemoveAsync($"{CACHE_KEY_POPULAR_TEMPLATES}{limit}", cancellationToken);
            }

            return _mapper.Map<ProductDto>(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product from template. TemplateId: {TemplateId}", request.TemplateId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
