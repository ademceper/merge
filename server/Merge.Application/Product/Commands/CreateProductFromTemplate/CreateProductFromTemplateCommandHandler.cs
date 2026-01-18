using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using System.Text.Json;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.CreateProductFromTemplate;

public class CreateProductFromTemplateCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<CreateProductFromTemplateCommandHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings,
    IMapper mapper) : IRequestHandler<CreateProductFromTemplateCommand, ProductDto>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    private const string CACHE_KEY_TEMPLATE_BY_ID = "product_template_";
    private const string CACHE_KEY_ALL_TEMPLATES = "product_templates_all";
    private const string CACHE_KEY_TEMPLATES_BY_CATEGORY = "product_templates_by_category_";
    private const string CACHE_KEY_POPULAR_TEMPLATES = "product_templates_popular_";

    public async Task<ProductDto> Handle(CreateProductFromTemplateCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating product from template. TemplateId: {TemplateId}, SellerId: {SellerId}",
            request.TemplateId, request.SellerId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var template = await context.Set<ProductTemplate>()
                .AsNoTracking()
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.IsActive, cancellationToken);

            if (template is null)
            {
                throw new NotFoundException("Şablon", request.TemplateId);
            }

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

            if (request.DiscountPrice.HasValue)
            {
                product.SetDiscountPrice(new Money(request.DiscountPrice.Value));
            }

            if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                product.UpdateImages(request.ImageUrl, request.ImageUrls ?? []);
            }
            else if (!string.IsNullOrEmpty(template.DefaultImageUrl))
            {
                product.UpdateImages(template.DefaultImageUrl, request.ImageUrls ?? []);
            }
            else if (request.ImageUrls is not null && request.ImageUrls.Any())
            {
                product.UpdateImages(request.ImageUrls.First(), request.ImageUrls);
            }

            await context.Set<ProductEntity>().AddAsync(product, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Increment template usage count
            var templateToUpdate = await context.Set<ProductTemplate>()
                .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

            if (templateToUpdate is not null)
            {
                templateToUpdate.IncrementUsageCount();
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            product = await context.Set<ProductEntity>()
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);

            if (product is null)
            {
                logger.LogError("Product not found after creation. ProductId: {ProductId}", product?.Id);
                throw new InvalidOperationException("Product could not be retrieved after creation");
            }

            logger.LogInformation("Product created from template successfully. ProductId: {ProductId}, TemplateId: {TemplateId}",
                product.Id, request.TemplateId);

            // Invalidate template cache (usage count changed)
            await cache.RemoveAsync($"{CACHE_KEY_TEMPLATE_BY_ID}{request.TemplateId}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_TEMPLATES, cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_TEMPLATES_BY_CATEGORY}{template.CategoryId}_", cancellationToken);
            // Invalidate popular templates cache (all possible limits)
            for (int limit = paginationConfig.DefaultPageSize; limit <= paginationConfig.MaxPageSize; limit += paginationConfig.DefaultPageSize)
            {
                await cache.RemoveAsync($"{CACHE_KEY_POPULAR_TEMPLATES}{limit}", cancellationToken);
            }

            return mapper.Map<ProductDto>(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product from template. TemplateId: {TemplateId}", request.TemplateId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
