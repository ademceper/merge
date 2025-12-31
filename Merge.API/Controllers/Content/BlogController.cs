using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Content;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;


namespace Merge.API.Controllers.Content;

[ApiController]
[Route("api/content/blog")]
public class BlogController : BaseController
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    // Categories
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<BlogCategoryDto>>> GetAllCategories([FromQuery] bool? isActive = null)
    {
        var categories = await _blogService.GetAllCategoriesAsync(isActive);
        return Ok(categories);
    }

    [HttpGet("categories/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<BlogCategoryDto>> GetCategory(Guid id)
    {
        var category = await _blogService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(category);
    }

    [HttpGet("categories/slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<BlogCategoryDto>> GetCategoryBySlug(string slug)
    {
        var category = await _blogService.GetCategoryBySlugAsync(slug);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(category);
    }

    [HttpPost("categories")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BlogCategoryDto>> CreateCategory([FromBody] CreateBlogCategoryDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var category = await _blogService.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CreateBlogCategoryDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _blogService.UpdateCategoryAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var success = await _blogService.DeleteCategoryAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Posts
    [HttpGet("posts")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<BlogPostDto>>> GetPosts(
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? status = "Published",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (categoryId.HasValue)
        {
            var posts = await _blogService.GetPostsByCategoryAsync(categoryId.Value, status, page, pageSize);
            return Ok(posts);
        }

        var recentPosts = await _blogService.GetRecentPostsAsync(pageSize);
        return Ok(recentPosts);
    }

    [HttpGet("posts/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<BlogPostDto>> GetPost(Guid id, [FromQuery] bool trackView = true)
    {
        var post = await _blogService.GetPostByIdAsync(id, trackView);
        if (post == null)
        {
            return NotFound();
        }
        return Ok(post);
    }

    [HttpGet("posts/slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<BlogPostDto>> GetPostBySlug(string slug, [FromQuery] bool trackView = true)
    {
        var post = await _blogService.GetPostBySlugAsync(slug, trackView);
        if (post == null)
        {
            return NotFound();
        }
        return Ok(post);
    }

    [HttpGet("posts/featured")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<BlogPostDto>>> GetFeaturedPosts([FromQuery] int count = 5)
    {
        var posts = await _blogService.GetFeaturedPostsAsync(count);
        return Ok(posts);
    }

    [HttpGet("posts/recent")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<BlogPostDto>>> GetRecentPosts([FromQuery] int count = 10)
    {
        var posts = await _blogService.GetRecentPostsAsync(count);
        return Ok(posts);
    }

    [HttpGet("posts/search")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<BlogPostDto>>> SearchPosts(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var posts = await _blogService.SearchPostsAsync(query, page, pageSize);
        return Ok(posts);
    }

    [HttpPost("posts")]
    [Authorize(Roles = "Admin,Manager,Writer")]
    public async Task<ActionResult<BlogPostDto>> CreatePost([FromBody] CreateBlogPostDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var authorId = GetUserId();
        var post = await _blogService.CreatePostAsync(authorId, dto);
        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
    }

    [HttpPut("posts/{id}")]
    [Authorize(Roles = "Admin,Manager,Writer")]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] CreateBlogPostDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _blogService.UpdatePostAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("posts/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var success = await _blogService.DeletePostAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("posts/{id}/publish")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> PublishPost(Guid id)
    {
        var success = await _blogService.PublishPostAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Comments
    [HttpGet("posts/{postId}/comments")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<BlogCommentDto>>> GetPostComments(Guid postId, [FromQuery] bool? isApproved = true)
    {
        var comments = await _blogService.GetPostCommentsAsync(postId, isApproved);
        return Ok(comments);
    }

    [HttpPost("comments")]
    [AllowAnonymous]
    public async Task<ActionResult<BlogCommentDto>> CreateComment([FromBody] CreateBlogCommentDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = GetUserId();
        }

        var comment = await _blogService.CreateCommentAsync(userId, dto);
        return CreatedAtAction(nameof(GetPostComments), new { postId = comment.BlogPostId }, comment);
    }

    [HttpPost("comments/{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ApproveComment(Guid id)
    {
        var success = await _blogService.ApproveCommentAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("comments/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var success = await _blogService.DeleteCommentAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Analytics
    [HttpGet("analytics")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<BlogAnalyticsDto>> GetAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var analytics = await _blogService.GetBlogAnalyticsAsync(startDate, endDate);
        return Ok(analytics);
    }
}

