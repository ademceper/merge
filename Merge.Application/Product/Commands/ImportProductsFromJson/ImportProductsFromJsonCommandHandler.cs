using MediatR;
using Microsoft.EntityFrameworkCore;
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
public class ImportProductsFromJsonCommandHandler : IRequestHandler<ImportProductsFromJsonCommand, BulkProductImportResultDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<ImportProductsFromJsonCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_PRODUCT_BY_ID = "product_";
    private const string CACHE_KEY_ALL_PRODUCTS_PAGED = "products_all_paged";
    private const string CACHE_KEY_PRODUCTS_BY_CATEGORY = "products_by_category_";
    private const string CACHE_KEY_PRODUCTS_SEARCH = "products_search_";

    public ImportProductsFromJsonCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper,
        ILogger<ImportProductsFromJsonCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<BulkProductImportResultDto> Handle(ImportProductsFromJsonCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("JSON bulk import başlatıldı");

        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var errors = new List<string>();
        var importedProducts = new List<ProductDto>();
        int totalProcessed = 0;
        int successCount = 0;
        int failureCount = 0;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
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
                        var importedProductDto = _mapper.Map<ProductDto>(product);
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
                    _logger.LogWarning(ex, "JSON import hatası. Product: {ProductName}", productDto.Name);
                }
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation - Bulk import sonrası tüm product cache'lerini invalidate et
            // Note: Pattern-based invalidation gerektirir. Şimdilik tüm product cache'lerini invalidate ediyoruz
            await _cache.RemoveAsync(CACHE_KEY_ALL_PRODUCTS_PAGED, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_PRODUCTS_SEARCH, cancellationToken);
            // Category-based cache'ler pattern-based invalidation gerektirir, şimdilik expiration'a güveniyoruz

            _logger.LogInformation(
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
            _logger.LogError(ex, "Error during JSON bulk import");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<ProductEntity?> ImportSingleProductAsync(BulkProductImportDto dto, CancellationToken cancellationToken)
    {
        // Check if SKU already exists
        var existingProduct = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.SKU == dto.SKU, cancellationToken);

        if (existingProduct != null)
        {
            _logger.LogWarning("SKU already exists: {SKU}", dto.SKU);
            return null;
        }

        var category = await _context.Set<Category>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == dto.CategoryName, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryName}", dto.CategoryName);
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

        await _context.Set<ProductEntity>().AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product;
    }
}
