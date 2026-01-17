using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using System.Text.Json;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.ImportProductsFromJson;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ImportProductsFromJsonCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ImportProductsFromJsonCommandHandler> logger,
    ICacheService cache,
    IMapper mapper) : IRequestHandler<ImportProductsFromJsonCommand, BulkProductImportResultDto>
{

    private const string CACHE_KEY_PRODUCT_BY_ID = "product_";
    private const string CACHE_KEY_ALL_PRODUCTS_PAGED = "products_all_paged";
    private const string CACHE_KEY_PRODUCTS_BY_CATEGORY = "products_by_category_";
    private const string CACHE_KEY_PRODUCTS_SEARCH = "products_search_";

    public async Task<BulkProductImportResultDto> Handle(ImportProductsFromJsonCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("JSON bulk import başlatıldı");

        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var errors = new List<string>();
        var importedProducts = new List<ProductDto>();
        int totalProcessed = 0;
        int successCount = 0;
        int failureCount = 0;

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var products = await JsonSerializer.DeserializeAsync<List<BulkProductImportDto>>(request.FileStream, cancellationToken: cancellationToken);

            if (products == null || products.Count == 0)
            {
                errors.Add("No products found in JSON file");
                return new BulkProductImportResultDto(
                    TotalProcessed: totalProcessed,
                    SuccessCount: successCount,
                    FailureCount: failureCount,
                    Errors: errors.AsReadOnly(),
                    ImportedProducts: importedProducts.AsReadOnly()
                );
            }

            foreach (var productDto in products)
            {
                if (cancellationToken.IsCancellationRequested) break;

                totalProcessed++;

                try
                {
                    var product = await ImportSingleProductAsync(productDto, cancellationToken);
                    if (product != null)
                    {
                        successCount++;
                        var importedProductDto = mapper.Map<ProductDto>(product);
                        importedProducts.Add(importedProductDto);
                    }
                    else
                    {
                        failureCount++;
                        errors.Add($"Failed to import product '{productDto.Name}'");
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    errors.Add($"Product '{productDto.Name}': {ex.Message}");
                    logger.LogWarning(ex, "JSON import hatası. Product: {ProductName}", productDto.Name);
                }
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation - Bulk import sonrası tüm product cache'lerini invalidate et
            // Note: Pattern-based invalidation gerektirir. Şimdilik tüm product cache'lerini invalidate ediyoruz
            await cache.RemoveAsync(CACHE_KEY_ALL_PRODUCTS_PAGED, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_PRODUCTS_SEARCH, cancellationToken);
            // Category-based cache'ler pattern-based invalidation gerektirir, şimdilik expiration'a güveniyoruz

            logger.LogInformation(
                "JSON bulk import tamamlandı. TotalProcessed: {TotalProcessed}, SuccessCount: {SuccessCount}, FailureCount: {FailureCount}",
                totalProcessed, successCount, failureCount);

            // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
            return new BulkProductImportResultDto(
                TotalProcessed: totalProcessed,
                SuccessCount: successCount,
                FailureCount: failureCount,
                Errors: errors.AsReadOnly(),
                ImportedProducts: importedProducts.AsReadOnly()
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during JSON bulk import");
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<ProductEntity?> ImportSingleProductAsync(BulkProductImportDto dto, CancellationToken cancellationToken)
    {
        // Check if SKU already exists
        var existingProduct = await context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.SKU == dto.SKU, cancellationToken);

        if (existingProduct != null)
        {
            logger.LogWarning("SKU already exists: {SKU}", dto.SKU);
            return null;
        }

        var category = await context.Set<Category>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == dto.CategoryName, cancellationToken);

        if (category == null)
        {
            logger.LogWarning("Category not found: {CategoryName}", dto.CategoryName);
            return null;
        }

        var sku = new SKU(dto.SKU);
        var price = new Money(dto.Price);
        var product = ProductEntity.Create(
            dto.Name,
            dto.Description,
            sku,
            price,
            dto.StockQuantity,
            category.Id,
            dto.Brand
        );

        if (dto.DiscountPrice.HasValue)
        {
            product.SetDiscountPrice(new Money(dto.DiscountPrice.Value));
        }

        if (!string.IsNullOrEmpty(dto.ImageUrl))
        {
            product.SetImageUrl(dto.ImageUrl);
        }

        if (!dto.IsActive)
        {
            product.Deactivate();
        }

        await context.Set<ProductEntity>().AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product;
    }
}
