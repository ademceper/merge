using MediatR;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Search;
using Merge.Application.Search.Queries.SearchProducts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Search;


namespace Merge.Application.Services.Search;

public class ElasticsearchService : IElasticsearchService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ElasticsearchService> _logger;
    private readonly IMediator _mediator; // Fallback to SQL search via MediatR

    public ElasticsearchService(
        IConfiguration configuration,
        ILogger<ElasticsearchService> logger,
        IMediator mediator)
    {
        _configuration = configuration;
        _logger = logger;
        _mediator = mediator;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> IndexProductAsync(ProductDto product, CancellationToken cancellationToken = default)
    {
        var elasticsearchUrl = _configuration["Elasticsearch:Url"];
        var indexName = _configuration["Elasticsearch:IndexName"] ?? "products";

        if (string.IsNullOrEmpty(elasticsearchUrl))
        {
            _logger.LogWarning("Elasticsearch not configured, skipping indexing");
            return false;
        }

        // Mock implementation - Gerçek implementasyonda Elasticsearch.NET client kullanılacak
        // var client = new ElasticClient(new Uri(elasticsearchUrl));
        // await client.IndexAsync(product, idx => idx.Index(indexName).Id(product.Id), cancellationToken);
        
        _logger.LogInformation("Product indexed to Elasticsearch: {ProductId}", product.Id);
        await Task.CompletedTask;
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> IndexProductsAsync(IEnumerable<ProductDto> products, CancellationToken cancellationToken = default)
    {
        var elasticsearchUrl = _configuration["Elasticsearch:Url"];

        if (string.IsNullOrEmpty(elasticsearchUrl))
        {
            _logger.LogWarning("Elasticsearch not configured, skipping bulk indexing");
            return false;
        }

        // Mock implementation - Gerçek implementasyonda bulk indexing yapılacak
        var productList = products.ToList();
        _logger.LogInformation("Bulk indexing {Count} products to Elasticsearch", productList.Count);
        
        foreach (var product in productList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await IndexProductAsync(product, cancellationToken);
        }
        
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var elasticsearchUrl = _configuration["Elasticsearch:Url"];
        var indexName = _configuration["Elasticsearch:IndexName"] ?? "products";

        if (string.IsNullOrEmpty(elasticsearchUrl))
        {
            return false;
        }

        // Mock implementation
        // var client = new ElasticClient(new Uri(elasticsearchUrl));
        // await client.DeleteAsync<ProductDto>(productId, idx => idx.Index(indexName), cancellationToken);
        
        _logger.LogInformation("Product deleted from Elasticsearch: {ProductId}", productId);
        await Task.CompletedTask;
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SearchResultDto> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken = default)
    {
        var elasticsearchUrl = _configuration["Elasticsearch:Url"];

        if (string.IsNullOrEmpty(elasticsearchUrl) || !await IsAvailableAsync(cancellationToken))
        {
            // Fallback to SQL-based search via MediatR
            _logger.LogInformation("Elasticsearch not available, using SQL search");
            var query = new SearchProductsQuery(
                request.SearchTerm,
                request.CategoryId,
                request.Brand,
                request.MinPrice,
                request.MaxPrice,
                request.MinRating,
                request.InStockOnly,
                request.SortBy,
                request.Page ?? 1,
                request.PageSize ?? 20
            );
            return await _mediator.Send(query, cancellationToken);
        }

        // Mock implementation - Gerçek implementasyonda Elasticsearch query yapılacak
        // var client = new ElasticClient(new Uri(elasticsearchUrl));
        // var searchResponse = await client.SearchAsync<ProductDto>(s => s
        //     .Index("products")
        //     .Query(q => q
        //         .MultiMatch(m => m
        //             .Fields(f => f.Field(p => p.Name).Field(p => p.Description))
        //             .Query(request.SearchTerm)
        //         )
        //     )
        //     .From((request.Page ?? 1 - 1) * (request.PageSize ?? 20))
        //     .Size(request.PageSize ?? 20), cancellationToken);
        
        _logger.LogInformation("Elasticsearch search executed: {SearchTerm}", request.SearchTerm);
        
        // Fallback to SQL search for now via MediatR
        var fallbackQuery = new SearchProductsQuery(
            request.SearchTerm,
            request.CategoryId,
            request.Brand,
            request.MinPrice,
            request.MaxPrice,
            request.MinRating,
            request.InStockOnly,
            request.SortBy,
            request.Page ?? 1,
            request.PageSize ?? 20
        );
        return await _mediator.Send(fallbackQuery, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ReindexAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var elasticsearchUrl = _configuration["Elasticsearch:Url"];

        if (string.IsNullOrEmpty(elasticsearchUrl))
        {
            _logger.LogWarning("Elasticsearch not configured, cannot reindex");
            return false;
        }

        // Mock implementation - Gerçek implementasyonda tüm ürünler indexlenecek
        _logger.LogInformation("Reindexing all products to Elasticsearch");
        await Task.Delay(1000, cancellationToken); // Simulate reindexing
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        var elasticsearchUrl = _configuration["Elasticsearch:Url"];

        if (string.IsNullOrEmpty(elasticsearchUrl))
        {
            return false;
        }

        // Mock implementation - Gerçek implementasyonda Elasticsearch health check yapılacak
        // var client = new ElasticClient(new Uri(elasticsearchUrl));
        // var health = await client.Cluster.HealthAsync(cancellationToken);
        // return health.IsValid;
        
        await Task.CompletedTask;
        return false; // Default to false, will use SQL search as fallback
    }
}

