using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Search;


namespace Merge.Application.Services.Search;

public class ElasticsearchService : IElasticsearchService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ElasticsearchService> _logger;
    private readonly IProductSearchService _productSearchService; // Fallback to SQL search

    public ElasticsearchService(
        IConfiguration configuration,
        ILogger<ElasticsearchService> logger,
        IProductSearchService productSearchService)
    {
        _configuration = configuration;
        _logger = logger;
        _productSearchService = productSearchService;
    }

    public async Task<bool> IndexProductAsync(ProductDto product)
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
        // await client.IndexAsync(product, idx => idx.Index(indexName).Id(product.Id));
        
        _logger.LogInformation("Product indexed to Elasticsearch: {ProductId}", product.Id);
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> IndexProductsAsync(IEnumerable<ProductDto> products)
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
            await IndexProductAsync(product);
        }
        
        return true;
    }

    public async Task<bool> DeleteProductAsync(Guid productId)
    {
        var elasticsearchUrl = _configuration["Elasticsearch:Url"];
        var indexName = _configuration["Elasticsearch:IndexName"] ?? "products";

        if (string.IsNullOrEmpty(elasticsearchUrl))
        {
            return false;
        }

        // Mock implementation
        // var client = new ElasticClient(new Uri(elasticsearchUrl));
        // await client.DeleteAsync<ProductDto>(productId, idx => idx.Index(indexName));
        
        _logger.LogInformation("Product deleted from Elasticsearch: {ProductId}", productId);
        await Task.CompletedTask;
        return true;
    }

    public async Task<SearchResultDto> SearchAsync(SearchRequestDto request)
    {
        var elasticsearchUrl = _configuration["Elasticsearch:Url"];

        if (string.IsNullOrEmpty(elasticsearchUrl) || !await IsAvailableAsync())
        {
            // Fallback to SQL-based search
            _logger.LogInformation("Elasticsearch not available, using SQL search");
            return await _productSearchService.SearchAsync(request);
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
        //     .Size(request.PageSize ?? 20)
        // );
        
        _logger.LogInformation("Elasticsearch search executed: {SearchTerm}", request.SearchTerm);
        
        // Fallback to SQL search for now
        return await _productSearchService.SearchAsync(request);
    }

    public async Task<bool> ReindexAllProductsAsync()
    {
        var elasticsearchUrl = _configuration["Elasticsearch:Url"];

        if (string.IsNullOrEmpty(elasticsearchUrl))
        {
            _logger.LogWarning("Elasticsearch not configured, cannot reindex");
            return false;
        }

        // Mock implementation - Gerçek implementasyonda tüm ürünler indexlenecek
        _logger.LogInformation("Reindexing all products to Elasticsearch");
        await Task.Delay(1000); // Simulate reindexing
        return true;
    }

    public async Task<bool> IsAvailableAsync()
    {
        var elasticsearchUrl = _configuration["Elasticsearch:Url"];

        if (string.IsNullOrEmpty(elasticsearchUrl))
        {
            return false;
        }

        // Mock implementation - Gerçek implementasyonda Elasticsearch health check yapılacak
        // var client = new ElasticClient(new Uri(elasticsearchUrl));
        // var health = await client.Cluster.HealthAsync();
        // return health.IsValid;
        
        await Task.CompletedTask;
        return false; // Default to false, will use SQL search as fallback
    }
}

