using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Search;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Search;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Search;

public class ProductSearchService(IDbContext context, IMapper mapper, ILogger<ProductSearchService> logger) : IProductSearchService
{

    public async Task<SearchResultDto> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Product search yapılıyor. SearchTerm: {SearchTerm}, CategoryId: {CategoryId}, Page: {Page}, PageSize: {PageSize}",
            request.SearchTerm, request.CategoryId, request.Page, request.PageSize);

        var query = context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .AsQueryable();

        // Arama terimi
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(p => 
                p.Name.Contains(request.SearchTerm) || 
                p.Description.Contains(request.SearchTerm) ||
                p.Brand.Contains(request.SearchTerm) ||
                p.SKU.Contains(request.SearchTerm));
        }

        // Kategori filtresi
        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        // Marka filtresi
        if (!string.IsNullOrEmpty(request.Brand))
        {
            query = query.Where(p => p.Brand == request.Brand);
        }

        // Fiyat aralığı
        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => (p.DiscountPrice ?? p.Price) >= request.MinPrice.Value);
        }
        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => (p.DiscountPrice ?? p.Price) <= request.MaxPrice.Value);
        }

        // Rating filtresi
        if (request.MinRating.HasValue)
        {
            query = query.Where(p => p.Rating >= request.MinRating.Value);
        }

        // Stok durumu
        if (request.InStockOnly)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        // Sayfalama
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;
        
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;
        
        // Toplam kayıt sayısı
        var totalCount = await query.CountAsync(cancellationToken);
        
        // Not: ProductDto için AutoMapper mapping'i kullanılmalı, ancak CategoryName için ForMember gerekli
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        var productDtos = mapper.Map<IEnumerable<ProductDto>>(products).ToList();

        var rankedProducts = ApplySearchRanking(productDtos, request.SearchTerm ?? string.Empty, request.SortBy);

        var brands = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Brand))
            .Select(p => p.Brand)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync(cancellationToken);

        var minPrice = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive)
            .MinAsync(p => (decimal?)p.Price, cancellationToken) ?? 0;

        var maxPrice = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.IsActive)
            .MaxAsync(p => (decimal?)p.Price, cancellationToken) ?? 0;

        logger.LogInformation(
            "Product search tamamlandı. TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}",
            totalCount, page, pageSize);

        return new SearchResultDto(
            Products: rankedProducts,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: (int)Math.Ceiling(totalCount / (double)pageSize),
            AvailableBrands: brands,
            MinPrice: minPrice,
            MaxPrice: maxPrice
        );
    }

    private List<ProductDto> ApplySearchRanking(List<ProductDto> products, string searchTerm, string? sortBy)
    {
        var sortByNorm = string.IsNullOrEmpty(sortBy) ? null : sortBy.ToLowerInvariant();
        if (sortByNorm is not null && sortByNorm != "relevance")
        {
            return sortByNorm switch
            {
                "price_asc" => products.OrderBy(p => p.DiscountPrice ?? p.Price).ToList(),
                "price_desc" => products.OrderByDescending(p => p.DiscountPrice ?? p.Price).ToList(),
                "rating" => products.OrderByDescending(p => p.Rating).ToList(),
                "newest" => products.OrderByDescending(p => p.Id).ToList(), // Using Id as proxy for creation date
                "popular" => products.OrderByDescending(p => p.ReviewCount).ToList(),
                _ => products
            };
        }

        // Relevance ranking algoritması
        if (string.IsNullOrEmpty(searchTerm))
        {
            // Arama terimi yoksa, popülerlik ve rating'e göre sırala
            return products.OrderByDescending(p => CalculateRelevanceScore(p, string.Empty)).ToList();
        }

        // Arama terimi varsa, relevance score hesapla
        var ranked = products
            .Select(p => new { Product = p, Score = CalculateRelevanceScore(p, searchTerm) })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Product)
            .ToList();

        return ranked;
    }

    private double CalculateRelevanceScore(ProductDto product, string searchTerm)
    {
        double score = 0;

        if (string.IsNullOrEmpty(searchTerm))
        {
            // Popülerlik bazlı scoring
            score += product.ReviewCount * 0.1; // Her review 0.1 puan
            score += (double)(product.Rating * 10); // Rating * 10
            score += product.StockQuantity > 0 ? 5 : 0; // Stokta varsa bonus
            score += product.DiscountPrice.HasValue ? 3 : 0; // İndirimli ürün bonusu
            return score;
        }

        // İsim eşleşmesi (en yüksek ağırlık)
        if (product.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        {
            score += 100;
            if (product.Name.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase))
            {
                score += 50; // Başlangıçta eşleşme bonusu
            }
        }

        // Tam eşleşme bonusu
        if (string.Equals(product.Name, searchTerm, StringComparison.OrdinalIgnoreCase))
        {
            score += 200;
        }

        // Marka eşleşmesi
        if ((product.Brand ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        {
            score += 30;
        }

        // SKU eşleşmesi
        if (product.SKU.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        {
            score += 20;
        }

        // Açıklama eşleşmesi (düşük ağırlık)
        if ((product.Description ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        {
            score += 10;
        }

        // Popülerlik faktörleri
        score += product.ReviewCount * 0.1;
        score += (double)(product.Rating * 5);
        score += product.StockQuantity > 0 ? 5 : -10; // Stokta yoksa ceza
        score += product.DiscountPrice.HasValue ? 2 : 0;

        return score;
    }
}

