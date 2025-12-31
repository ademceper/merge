using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Support;
using Merge.Application.DTOs.Support;

namespace Merge.API.Controllers.Support;

[ApiController]
[Route("api/support/knowledge-base")]
public class KnowledgeBaseController : BaseController
{
    private readonly IKnowledgeBaseService _knowledgeBaseService;

    public KnowledgeBaseController(IKnowledgeBaseService knowledgeBaseService)
    {
        _knowledgeBaseService = knowledgeBaseService;
    }

    // Articles - Public
    [HttpGet("articles")]
    public async Task<ActionResult<IEnumerable<KnowledgeBaseArticleDto>>> GetArticles(
        [FromQuery] string? status = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool featuredOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var articles = await _knowledgeBaseService.GetArticlesAsync(status, categoryId, featuredOnly, page, pageSize);
        return Ok(articles);
    }

    [HttpGet("articles/{id}")]
    public async Task<ActionResult<KnowledgeBaseArticleDto>> GetArticle(Guid id)
    {
        var article = await _knowledgeBaseService.GetArticleAsync(id);
        if (article == null)
        {
            return NotFound();
        }
        return Ok(article);
    }

    [HttpGet("articles/slug/{slug}")]
    public async Task<ActionResult<KnowledgeBaseArticleDto>> GetArticleBySlug(string slug)
    {
        var article = await _knowledgeBaseService.GetArticleBySlugAsync(slug);
        if (article == null)
        {
            return NotFound();
        }

        // Record view
        var userId = GetUserIdOrNull();
        await _knowledgeBaseService.RecordArticleViewAsync(article.Id, userId, HttpContext.Connection.RemoteIpAddress?.ToString());

        return Ok(article);
    }

    [HttpPost("articles/search")]
    public async Task<ActionResult<IEnumerable<KnowledgeBaseArticleDto>>> SearchArticles([FromBody] KnowledgeBaseSearchDto searchDto)
    {
        var articles = await _knowledgeBaseService.SearchArticlesAsync(searchDto);
        return Ok(articles);
    }

    // Articles - Admin
    [HttpPost("articles")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<KnowledgeBaseArticleDto>> CreateArticle([FromBody] CreateKnowledgeBaseArticleDto dto)
    {
        var authorId = GetUserId();
        var article = await _knowledgeBaseService.CreateArticleAsync(dto, authorId);
        return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, article);
    }

    [HttpPut("articles/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<KnowledgeBaseArticleDto>> UpdateArticle(Guid id, [FromBody] UpdateKnowledgeBaseArticleDto dto)
    {
        var article = await _knowledgeBaseService.UpdateArticleAsync(id, dto);
        if (article == null)
        {
            return NotFound();
        }
        return Ok(article);
    }

    [HttpDelete("articles/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteArticle(Guid id)
    {
        var success = await _knowledgeBaseService.DeleteArticleAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return Ok();
    }

    [HttpPost("articles/{id}/publish")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> PublishArticle(Guid id)
    {
        var success = await _knowledgeBaseService.PublishArticleAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return Ok();
    }

    // Categories - Public
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<KnowledgeBaseCategoryDto>>> GetCategories([FromQuery] bool includeSubCategories = true)
    {
        var categories = await _knowledgeBaseService.GetCategoriesAsync(includeSubCategories);
        return Ok(categories);
    }

    [HttpGet("categories/{id}")]
    public async Task<ActionResult<KnowledgeBaseCategoryDto>> GetCategory(Guid id)
    {
        var category = await _knowledgeBaseService.GetCategoryAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(category);
    }

    [HttpGet("categories/slug/{slug}")]
    public async Task<ActionResult<KnowledgeBaseCategoryDto>> GetCategoryBySlug(string slug)
    {
        var category = await _knowledgeBaseService.GetCategoryBySlugAsync(slug);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(category);
    }

    // Categories - Admin
    [HttpPost("categories")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<KnowledgeBaseCategoryDto>> CreateCategory([FromBody] CreateKnowledgeBaseCategoryDto dto)
    {
        var category = await _knowledgeBaseService.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<KnowledgeBaseCategoryDto>> UpdateCategory(Guid id, [FromBody] UpdateKnowledgeBaseCategoryDto dto)
    {
        var category = await _knowledgeBaseService.UpdateCategoryAsync(id, dto);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(category);
    }

    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var success = await _knowledgeBaseService.DeleteCategoryAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return Ok();
    }

    // Stats
    [HttpGet("stats/articles")]
    public async Task<ActionResult<int>> GetArticleCount([FromQuery] Guid? categoryId = null)
    {
        var count = await _knowledgeBaseService.GetArticleCountAsync(categoryId);
        return Ok(new { count });
    }

    [HttpGet("stats/views")]
    public async Task<ActionResult<int>> GetTotalViews([FromQuery] Guid? articleId = null)
    {
        var views = await _knowledgeBaseService.GetTotalViewsAsync(articleId);
        return Ok(new { views });
    }
}

