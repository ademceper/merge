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

namespace Merge.API.Controllers.Support;

[ApiVersion("1.0")]
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/support/knowledge-base")]
public class KnowledgeBaseController(IMediator mediator, IOptions<SupportSettings> supportSettings) : BaseController
{
    private readonly SupportSettings _supportSettings = supportSettings.Value;

    // Articles - Public
                    [HttpGet("articles")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(PagedResult<KnowledgeBaseArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<KnowledgeBaseArticleDto>>> GetArticles(
        [FromQuery] string? status = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool featuredOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
                                if (pageSize <= 0) pageSize = _supportSettings.DefaultPageSize;
        if (pageSize > _supportSettings.MaxPageSize) pageSize = _supportSettings.MaxPageSize;
        if (page < 1) page = 1;

                var query = new GetKnowledgeBaseArticlesQuery(status, categoryId, featuredOnly, page, pageSize);
        var articles = await mediator.Send(query, cancellationToken);
        
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

    [HttpGet("articles/{id}", Name = "GetArticle")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseArticleDto>> GetArticle(
        Guid id,
        CancellationToken cancellationToken = default)
    {
                        var query = new GetKnowledgeBaseArticleQuery(id);
        var article = await mediator.Send(query, cancellationToken);
        if (article == null)
        {
            return NotFound();
        }
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateKnowledgeBaseArticleLinks(Url, article.Id, version);
        article = article with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return Ok(article);
    }

    [HttpGet("articles/slug/{slug}")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseArticleDto>> GetArticleBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
                        var query = new GetKnowledgeBaseArticleBySlugQuery(slug);
        var article = await mediator.Send(query, cancellationToken);
        if (article == null)
        {
            return NotFound();
        }

        // Record view
        var userId = GetUserIdOrNull();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        var recordCommand = new RecordKnowledgeBaseArticleViewCommand(article.Id, userId, ipAddress, userAgent);
        await mediator.Send(recordCommand, cancellationToken);

                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateKnowledgeBaseArticleLinks(Url, article.Id, version);
        article = article with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };

        return Ok(article);
    }

    /// <summary>
    /// Knowledge base makalelerinde arama (GET endpoint - REST best practice)
    /// HIGH-API-004: POST yerine GET kullanımı - Search operations should use GET with query params
    /// </summary>
    [HttpGet("articles/search")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<KnowledgeBaseArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<KnowledgeBaseArticleDto>>> SearchArticles(
        [FromQuery] string? query = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool featuredOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var searchQuery = new SearchKnowledgeBaseArticlesQuery(
            query ?? string.Empty,
            categoryId,
            featuredOnly,
            page < 1 ? 1 : page,
            pageSize > 100 ? 100 : pageSize);
        var articles = await mediator.Send(searchQuery, cancellationToken);
        
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
            new { query, categoryId, featuredOnly },
            version);
        articles.Items = updatedItems;
        articles.Links = paginationLinks.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        
        return Ok(articles);
    }

    // Articles - Admin
                [HttpPost("articles")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]     [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseArticleDto>> CreateArticle(
        [FromBody] CreateKnowledgeBaseArticleDto dto,
        CancellationToken cancellationToken = default)
    {
                var authorId = GetUserId();
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
        var article = await mediator.Send(command, cancellationToken);
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateKnowledgeBaseArticleLinks(Url, article.Id, version);
        article = article with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return CreatedAtAction(nameof(GetArticle), new { version, id = article.Id }, article);
    }

    [HttpPut("articles/{id}", Name = "UpdateArticle")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]     [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status200OK)]
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
        var article = await mediator.Send(command, cancellationToken);
        if (article == null)
        {
            return NotFound();
        }
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateKnowledgeBaseArticleLinks(Url, article.Id, version);
        article = article with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return Ok(article);
    }

    /// <summary>
    /// Bilgi bankası makalesini kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("articles/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(KnowledgeBaseArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseArticleDto>> PatchArticle(
        Guid id,
        [FromBody] PatchKnowledgeBaseArticleDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateKnowledgeBaseArticleCommand(
            id,
            patchDto.Title,
            patchDto.Content,
            patchDto.Excerpt,
            patchDto.CategoryId,
            patchDto.Status,
            patchDto.IsFeatured,
            patchDto.DisplayOrder,
            patchDto.Tags);
        var article = await mediator.Send(command, cancellationToken);
        if (article == null)
        {
            return NotFound();
        }

        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateKnowledgeBaseArticleLinks(Url, article.Id, version);
        article = article with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };

        return Ok(article);
    }

    [HttpDelete("articles/{id}", Name = "DeleteArticle")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]     [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteArticle(
        Guid id,
        CancellationToken cancellationToken = default)
    {
                        var command = new DeleteKnowledgeBaseArticleCommand(id);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("articles/{id}/publish", Name = "PublishArticle")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]     [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PublishArticle(
        Guid id,
        CancellationToken cancellationToken = default)
    {
                        var command = new PublishKnowledgeBaseArticleCommand(id);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Categories - Public
                [HttpGet("categories")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(IEnumerable<KnowledgeBaseCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<KnowledgeBaseCategoryDto>>> GetCategories(
        [FromQuery] bool includeSubCategories = true,
        CancellationToken cancellationToken = default)
    {
                        var query = new GetKnowledgeBaseCategoriesQuery(includeSubCategories);
        var categories = await mediator.Send(query, cancellationToken);
        
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

    [HttpGet("categories/{id}", Name = "GetCategory")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(KnowledgeBaseCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseCategoryDto>> GetCategory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
                        var query = new GetKnowledgeBaseCategoryQuery(id);
        var category = await mediator.Send(query, cancellationToken);
        if (category == null)
        {
            return NotFound();
        }
        
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

    [HttpGet("categories/slug/{slug}")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(KnowledgeBaseCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseCategoryDto>> GetCategoryBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
                        var query = new GetKnowledgeBaseCategoryBySlugQuery(slug);
        var category = await mediator.Send(query, cancellationToken);
        if (category == null)
        {
            return NotFound();
        }
        
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

    // Categories - Admin
                [HttpPost("categories")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]     [ProducesResponseType(typeof(KnowledgeBaseCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseCategoryDto>> CreateCategory(
        [FromBody] CreateKnowledgeBaseCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
                        var command = new CreateKnowledgeBaseCategoryCommand(
            dto.Name,
            dto.Description,
            dto.ParentCategoryId,
            dto.DisplayOrder,
            dto.IsActive,
            dto.IconUrl);
        var category = await mediator.Send(command, cancellationToken);
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateKnowledgeBaseCategoryLinks(Url, category.Id, version);
        category = category with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return CreatedAtAction(nameof(GetCategory), new { version, id = category.Id }, category);
    }

    [HttpPut("categories/{id}", Name = "UpdateCategory")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]     [ProducesResponseType(typeof(KnowledgeBaseCategoryDto), StatusCodes.Status200OK)]
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
                        var command = new UpdateKnowledgeBaseCategoryCommand(
            id,
            dto.Name,
            dto.Description,
            dto.ParentCategoryId,
            dto.DisplayOrder,
            dto.IsActive,
            dto.IconUrl);
        var category = await mediator.Send(command, cancellationToken);
        if (category == null)
        {
            return NotFound();
        }
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateKnowledgeBaseCategoryLinks(Url, category.Id, version);
        category = category with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return Ok(category);
    }

    /// <summary>
    /// Bilgi bankası kategorisini kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("categories/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(KnowledgeBaseCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<KnowledgeBaseCategoryDto>> PatchCategory(
        Guid id,
        [FromBody] PatchKnowledgeBaseCategoryDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateKnowledgeBaseCategoryCommand(
            id,
            patchDto.Name,
            patchDto.Description,
            patchDto.ParentCategoryId,
            patchDto.DisplayOrder,
            patchDto.IsActive,
            patchDto.IconUrl);
        var category = await mediator.Send(command, cancellationToken);
        if (category == null)
        {
            return NotFound();
        }

        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateKnowledgeBaseCategoryLinks(Url, category.Id, version);
        category = category with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };

        return Ok(category);
    }

    [HttpDelete("categories/{id}", Name = "DeleteCategory")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]     [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteCategory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
                        var command = new DeleteKnowledgeBaseCategoryCommand(id);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Stats
                [HttpGet("stats/articles")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<int>> GetArticleCount(
        [FromQuery] Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
                        var query = new GetKnowledgeBaseArticleCountQuery(categoryId);
        var count = await mediator.Send(query, cancellationToken);
        return Ok(new { count });
    }

    [HttpGet("stats/views")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<int>> GetTotalViews(
        [FromQuery] Guid? articleId = null,
        CancellationToken cancellationToken = default)
    {
                        var query = new GetKnowledgeBaseTotalViewsQuery(articleId);
        var views = await mediator.Send(query, cancellationToken);
        return Ok(new { views });
    }
}
