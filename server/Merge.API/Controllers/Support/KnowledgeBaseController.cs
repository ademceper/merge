using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Support.Commands.CreateKnowledgeBaseArticle;
using Merge.Application.Support.Commands.UpdateKnowledgeBaseArticle;
using Merge.Application.Support.Commands.DeleteKnowledgeBaseArticle;
using Merge.Application.Support.Commands.PublishKnowledgeBaseArticle;
using Merge.Application.Support.Commands.RecordKnowledgeBaseArticleView;
using Merge.Application.Support.Commands.CreateKnowledgeBaseCategory;
using Merge.Application.Support.Commands.UpdateKnowledgeBaseCategory;
using Merge.Application.Support.Commands.DeleteKnowledgeBaseCategory;
using Merge.Application.Support.Queries.GetKnowledgeBaseArticle;
using Merge.Application.Support.Queries.GetKnowledgeBaseArticleBySlug;
using Merge.Application.Support.Queries.GetKnowledgeBaseArticles;
using Merge.Application.Support.Queries.SearchKnowledgeBaseArticles;
using Merge.Application.Support.Queries.GetKnowledgeBaseCategories;
using Merge.Application.Support.Queries.GetKnowledgeBaseCategory;
using Merge.Application.Support.Queries.GetKnowledgeBaseCategoryBySlug;
using Merge.Application.Support.Queries.GetKnowledgeBaseArticleCount;
using Merge.Application.Support.Queries.GetKnowledgeBaseTotalViews;
using Merge.API.Middleware;
using Merge.API.Helpers;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
// ✅ BOLUM 4.1.3: HATEOAS (ZORUNLU)
namespace Merge.API.Controllers.Support;

/// <summary>
/// Knowledge Base Controller - Manages knowledge base articles and categories
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/support/knowledge-base")]
public class KnowledgeBaseController : BaseController
{
    private readonly IMediator _mediator;
    private readonly SupportSettings _settings;

    public KnowledgeBaseController(
        IMediator mediator,
        IOptions<SupportSettings> settings)
    {
        _mediator = mediator;
        _settings = settings.Value;
    }

    /// <summary>
    /// Gets all knowledge base articles with optional filtering
    /// </summary>
    /// <param name="status">Filter by status (Published, Draft, etc.)</param>
    /// <param name="categoryId">Filter by category ID</param>
    /// <param name="featuredOnly">Return only featured articles</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of articles</returns>
    /// <response code="200">Articles retrieved successfully</response>
    /// <response code="429">Rate limit exceeded</response>
    // Articles - Public
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("articles")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<KnowledgeBaseArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<KnowledgeBaseArticleDto>>> GetArticles(
        [FromQuery] string? status = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool featuredOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
        if (pageSize <= 0) pageSize = _settings.DefaultPageSize;
        if (pageSize > _settings.MaxPageSize) pageSize = _settings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetKnowledgeBaseArticlesQuery(status, categoryId, featuredOnly, page, pageSize);
        var articles = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each article
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var updatedItems = articles.Items.Select(article =>
        {
            var links = HateoasHelper.CreateKnowledgeBaseArticleLinks(Url, article.Id, version);
            return article with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        }).ToList();
        
        // Add pagination links
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url,
            "GetArticles",
            articles.Page,
            articles.PageSize,
            articles.TotalPages,
            new { status, categoryId, featuredOnly },
            version);
        articles.Items = updatedItems;
        articles.Links = paginationLinks.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        
        return Ok(articles);
    }

    /// <summary>
    /// Gets a knowledge base article by ID
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The article</returns>
    /// <response code="200">Article found</response>
    /// <response code="404">Article not found</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("articles/{id}", Name = "GetArticle")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseArticleDto>> GetArticle(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetKnowledgeBaseArticleQuery(id);
        var article = await _mediator.Send(query, cancellationToken);
        if (article == null)
        {
            return NotFound();
        }
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateKnowledgeBaseArticleLinks(Url, article.Id, version);
        article = article with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return Ok(article);
    }

    /// <summary>
    /// Gets a knowledge base article by slug
    /// </summary>
    /// <param name="slug">Article slug</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The article</returns>
    /// <response code="200">Article found</response>
    /// <response code="404">Article not found</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("articles/slug/{slug}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseArticleDto>> GetArticleBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetKnowledgeBaseArticleBySlugQuery(slug);
        var article = await _mediator.Send(query, cancellationToken);
        if (article == null)
        {
            return NotFound();
        }

        // Record view
        var userId = GetUserIdOrNull();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        var recordCommand = new RecordKnowledgeBaseArticleViewCommand(article.Id, userId, ipAddress, userAgent);
        await _mediator.Send(recordCommand, cancellationToken);

        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateKnowledgeBaseArticleLinks(Url, article.Id, version);
        article = article with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };

        return Ok(article);
    }

    /// <summary>
    /// Searches knowledge base articles
    /// </summary>
    /// <param name="searchDto">Search criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching articles</returns>
    /// <response code="200">Search completed successfully</response>
    /// <response code="400">Invalid search criteria</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("articles/search")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<KnowledgeBaseArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<KnowledgeBaseArticleDto>>> SearchArticles(
        [FromBody] KnowledgeBaseSearchDto searchDto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new SearchKnowledgeBaseArticlesQuery(
            searchDto.Query,
            searchDto.CategoryId,
            searchDto.FeaturedOnly,
            searchDto.Page,
            searchDto.PageSize);
        var articles = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each article
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var updatedItems = articles.Items.Select(article =>
        {
            var links = HateoasHelper.CreateKnowledgeBaseArticleLinks(Url, article.Id, version);
            return article with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        }).ToList();
        
        // Add pagination links
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url,
            "SearchArticles",
            articles.Page,
            articles.PageSize,
            articles.TotalPages,
            new { query = searchDto.Query, categoryId = searchDto.CategoryId, featuredOnly = searchDto.FeaturedOnly },
            version);
        articles.Items = updatedItems;
        articles.Links = paginationLinks.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        
        return Ok(articles);
    }

    /// <summary>
    /// Creates a new knowledge base article
    /// </summary>
    /// <param name="dto">Article creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created article</returns>
    /// <response code="201">Article created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // Articles - Admin
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("articles")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseArticleDto>> CreateArticle(
        [FromBody] CreateKnowledgeBaseArticleDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var authorId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new CreateKnowledgeBaseArticleCommand(
            authorId,
            dto.Title,
            dto.Content,
            dto.Excerpt,
            dto.CategoryId,
            dto.Status,
            dto.IsFeatured,
            dto.DisplayOrder,
            dto.Tags?.ToList());
        var article = await _mediator.Send(command, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateKnowledgeBaseArticleLinks(Url, article.Id, version);
        article = article with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return CreatedAtAction(nameof(GetArticle), new { version, id = article.Id }, article);
    }

    /// <summary>
    /// Updates a knowledge base article
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <param name="dto">Article update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated article</returns>
    /// <response code="200">Article updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Article not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPut("articles/{id}", Name = "UpdateArticle")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseArticleDto>> UpdateArticle(
        Guid id,
        [FromBody] UpdateKnowledgeBaseArticleDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new UpdateKnowledgeBaseArticleCommand(
            id,
            dto.Title,
            dto.Content,
            dto.Excerpt,
            dto.CategoryId,
            dto.Status,
            dto.IsFeatured,
            dto.DisplayOrder,
            dto.Tags?.ToList());
        var article = await _mediator.Send(command, cancellationToken);
        if (article == null)
        {
            return NotFound();
        }
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateKnowledgeBaseArticleLinks(Url, article.Id, version);
        article = article with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return Ok(article);
    }

    /// <summary>
    /// Deletes a knowledge base article
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Article deleted successfully</response>
    /// <response code="404">Article not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpDelete("articles/{id}", Name = "DeleteArticle")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteArticle(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new DeleteKnowledgeBaseArticleCommand(id);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Publishes a knowledge base article
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Article published successfully</response>
    /// <response code="404">Article not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("articles/{id}/publish", Name = "PublishArticle")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PublishArticle(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new PublishKnowledgeBaseArticleCommand(id);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Gets all knowledge base categories
    /// </summary>
    /// <param name="includeSubCategories">Include subcategories in the response</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of categories</returns>
    /// <response code="200">Categories retrieved successfully</response>
    /// <response code="429">Rate limit exceeded</response>
    // Categories - Public
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("categories")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<KnowledgeBaseCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<KnowledgeBaseCategoryDto>>> GetCategories(
        [FromQuery] bool includeSubCategories = true,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetKnowledgeBaseCategoriesQuery(includeSubCategories);
        var categories = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each category
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        
        KnowledgeBaseCategoryDto AddLinksToCategory(KnowledgeBaseCategoryDto category)
        {
            var links = HateoasHelper.CreateKnowledgeBaseCategoryLinks(Url, category.Id, version);
            var updatedSubCategories = category.SubCategories?.Select(AddLinksToCategory).ToList().AsReadOnly() 
                ?? Array.Empty<KnowledgeBaseCategoryDto>().AsReadOnly();
            return category with 
            { 
                Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
                SubCategories = updatedSubCategories
            };
        }
        
        categories = categories.Select(AddLinksToCategory).ToList();
        
        return Ok(categories);
    }

    /// <summary>
    /// Gets a knowledge base category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The category</returns>
    /// <response code="200">Category found</response>
    /// <response code="404">Category not found</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("categories/{id}", Name = "GetCategory")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(KnowledgeBaseCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseCategoryDto>> GetCategory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetKnowledgeBaseCategoryQuery(id);
        var category = await _mediator.Send(query, cancellationToken);
        if (category == null)
        {
            return NotFound();
        }
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        
        KnowledgeBaseCategoryDto AddLinksToCategory(KnowledgeBaseCategoryDto cat)
        {
            var links = HateoasHelper.CreateKnowledgeBaseCategoryLinks(Url, cat.Id, version);
            var updatedSubCategories = cat.SubCategories?.Select(AddLinksToCategory).ToList().AsReadOnly() 
                ?? Array.Empty<KnowledgeBaseCategoryDto>().AsReadOnly();
            return cat with 
            { 
                Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
                SubCategories = updatedSubCategories
            };
        }
        
        category = AddLinksToCategory(category);
        
        return Ok(category);
    }

    /// <summary>
    /// Gets a knowledge base category by slug
    /// </summary>
    /// <param name="slug">Category slug</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The category</returns>
    /// <response code="200">Category found</response>
    /// <response code="404">Category not found</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("categories/slug/{slug}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(KnowledgeBaseCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseCategoryDto>> GetCategoryBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetKnowledgeBaseCategoryBySlugQuery(slug);
        var category = await _mediator.Send(query, cancellationToken);
        if (category == null)
        {
            return NotFound();
        }
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        
        KnowledgeBaseCategoryDto AddLinksToCategory(KnowledgeBaseCategoryDto cat)
        {
            var links = HateoasHelper.CreateKnowledgeBaseCategoryLinks(Url, cat.Id, version);
            var updatedSubCategories = cat.SubCategories?.Select(AddLinksToCategory).ToList().AsReadOnly() 
                ?? Array.Empty<KnowledgeBaseCategoryDto>().AsReadOnly();
            return cat with 
            { 
                Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
                SubCategories = updatedSubCategories
            };
        }
        
        category = AddLinksToCategory(category);
        
        return Ok(category);
    }

    /// <summary>
    /// Creates a new knowledge base category (Admin/Manager only)
    /// </summary>
    /// <param name="dto">Category creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created category</returns>
    /// <response code="201">Category created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // Categories - Admin
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("categories")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(KnowledgeBaseCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseCategoryDto>> CreateCategory(
        [FromBody] CreateKnowledgeBaseCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new CreateKnowledgeBaseCategoryCommand(
            dto.Name,
            dto.Description,
            dto.ParentCategoryId,
            dto.DisplayOrder,
            dto.IsActive,
            dto.IconUrl);
        var category = await _mediator.Send(command, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateKnowledgeBaseCategoryLinks(Url, category.Id, version);
        category = category with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return CreatedAtAction(nameof(GetCategory), new { version, id = category.Id }, category);
    }

    /// <summary>
    /// Updates a knowledge base category (Admin/Manager only)
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="dto">Category update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated category</returns>
    /// <response code="200">Category updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Category not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPut("categories/{id}", Name = "UpdateCategory")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(KnowledgeBaseCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseCategoryDto>> UpdateCategory(
        Guid id,
        [FromBody] UpdateKnowledgeBaseCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new UpdateKnowledgeBaseCategoryCommand(
            id,
            dto.Name,
            dto.Description,
            dto.ParentCategoryId,
            dto.DisplayOrder,
            dto.IsActive,
            dto.IconUrl);
        var category = await _mediator.Send(command, cancellationToken);
        if (category == null)
        {
            return NotFound();
        }
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateKnowledgeBaseCategoryLinks(Url, category.Id, version);
        category = category with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return Ok(category);
    }

    /// <summary>
    /// Deletes a knowledge base category (Admin/Manager only)
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Category deleted successfully</response>
    /// <response code="404">Category not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpDelete("categories/{id}", Name = "DeleteCategory")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteCategory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new DeleteKnowledgeBaseCategoryCommand(id);
        var success = await _mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Stats
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("stats/articles")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<int>> GetArticleCount(
        [FromQuery] Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetKnowledgeBaseArticleCountQuery(categoryId);
        var count = await _mediator.Send(query, cancellationToken);
        return Ok(new { count });
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("stats/views")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<int>> GetTotalViews(
        [FromQuery] Guid? articleId = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetKnowledgeBaseTotalViewsQuery(articleId);
        var views = await _mediator.Send(query, cancellationToken);
        return Ok(new { views });
    }
}

