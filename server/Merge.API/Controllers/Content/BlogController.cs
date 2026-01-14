using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.API.Middleware;
using Merge.Application.Content.Commands.CreateBlogCategory;
using Merge.Application.Content.Commands.UpdateBlogCategory;
using Merge.Application.Content.Commands.DeleteBlogCategory;
using Merge.Application.Content.Queries.GetAllBlogCategories;
using Merge.Application.Content.Queries.GetBlogCategoryById;
using Merge.Application.Content.Queries.GetBlogCategoryBySlug;
using Merge.Application.Content.Commands.CreateBlogPost;
using Merge.Application.Content.Commands.UpdateBlogPost;
using Merge.Application.Content.Commands.DeleteBlogPost;
using Merge.Application.Content.Commands.PublishBlogPost;
using Merge.Application.Content.Queries.GetBlogPosts;
using Merge.Application.Content.Queries.GetFeaturedBlogPosts;
using Merge.Application.Content.Queries.GetRecentBlogPosts;
using Merge.Application.Content.Queries.SearchBlogPosts;
using Merge.Application.Content.Queries.GetBlogPostById;
using Merge.Application.Content.Queries.GetBlogPostBySlug;
using Merge.Application.Content.Commands.CreateBlogComment;
using Merge.Application.Content.Commands.ApproveBlogComment;
using Merge.Application.Content.Commands.DeleteBlogComment;
using Merge.Application.Content.Queries.GetBlogPostComments;
using Merge.Application.Content.Queries.GetBlogAnalytics;

namespace Merge.API.Controllers.Content;

// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/content/blog")]
public class BlogController : BaseController
{
    private readonly IMediator _mediator;
    private readonly PaginationSettings _paginationSettings;

    public BlogController(
        IMediator mediator,
        IOptions<PaginationSettings> paginationSettings)
    {
        _mediator = mediator;
        _paginationSettings = paginationSettings.Value;
    }

    // Categories
    /// <summary>
    /// Tüm blog kategorilerini getirir
    /// </summary>
    /// <param name="isActive">Sadece aktif kategorileri getir (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Blog kategori listesi</returns>
    /// <response code="200">Blog kategorileri başarıyla getirildi</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("categories")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<BlogCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<BlogCategoryDto>>> GetAllCategories(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetAllBlogCategoriesQuery(isActive);
        var categories = await _mediator.Send(query, cancellationToken);
        return Ok(categories);
    }

    /// <summary>
    /// Blog kategori detaylarını getirir
    /// </summary>
    /// <param name="id">Kategori ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Blog kategori detayları</returns>
    /// <response code="200">Blog kategori başarıyla getirildi</response>
    /// <response code="404">Blog kategori bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("categories/{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(BlogCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BlogCategoryDto>> GetCategory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetBlogCategoryByIdQuery(id);
        var category = await _mediator.Send(query, cancellationToken);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(category);
    }

    /// <summary>
    /// Slug'a göre blog kategori getirir
    /// </summary>
    /// <param name="slug">Kategori slug'ı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Blog kategori detayları</returns>
    /// <response code="200">Blog kategori başarıyla getirildi</response>
    /// <response code="404">Blog kategori bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("categories/slug/{slug}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(BlogCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BlogCategoryDto>> GetCategoryBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetBlogCategoryBySlugQuery(slug);
        var category = await _mediator.Send(query, cancellationToken);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(category);
    }

    /// <summary>
    /// Yeni blog kategori oluşturur
    /// </summary>
    /// <param name="command">Blog kategori oluşturma komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan blog kategori</returns>
    /// <response code="201">Blog kategori başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("categories")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(BlogCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BlogCategoryDto>> CreateCategory(
        [FromBody] CreateBlogCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var category = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    /// <summary>
    /// Blog kategoriyi günceller
    /// </summary>
    /// <param name="id">Güncellenecek kategori ID</param>
    /// <param name="command">Blog kategori güncelleme komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Blog kategori başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Blog kategori bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateBlogCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var updateCommand = command with { Id = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Blog kategoriyi siler
    /// </summary>
    /// <param name="id">Silinecek kategori ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Blog kategori başarıyla silindi</response>
    /// <response code="404">Blog kategori bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteCategory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new DeleteBlogCategoryCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Posts
    /// <summary>
    /// Blog post'ları getirir (sayfalanmış)
    /// </summary>
    /// <param name="categoryId">Kategori ID (opsiyonel)</param>
    /// <param name="status">Post durumu (varsayılan: Published)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış blog post listesi</returns>
    /// <response code="200">Blog post'ları başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("posts")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<BlogPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<BlogPostDto>>> GetPosts(
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? status = "Published",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetBlogPostsQuery(categoryId, status, page, pageSize);
        var posts = await _mediator.Send(query, cancellationToken);
        return Ok(posts);
    }

    /// <summary>
    /// Blog post detaylarını getirir
    /// </summary>
    /// <param name="id">Blog post ID</param>
    /// <param name="trackView">Görüntülenme sayısını artır (varsayılan: true)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Blog post detayları</returns>
    /// <response code="200">Blog post başarıyla getirildi</response>
    /// <response code="404">Blog post bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("posts/{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BlogPostDto>> GetPost(
        Guid id,
        [FromQuery] bool trackView = true,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetBlogPostByIdQuery(id, trackView);
        var post = await _mediator.Send(query, cancellationToken);
        if (post == null)
        {
            return NotFound();
        }
        return Ok(post);
    }

    /// <summary>
    /// Slug'a göre blog post getirir
    /// </summary>
    /// <param name="slug">Blog post slug'ı</param>
    /// <param name="trackView">Görüntülenme sayısını artır (varsayılan: true)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Blog post detayları</returns>
    /// <response code="200">Blog post başarıyla getirildi</response>
    /// <response code="404">Blog post bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("posts/slug/{slug}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BlogPostDto>> GetPostBySlug(
        string slug,
        [FromQuery] bool trackView = true,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetBlogPostBySlugQuery(slug, trackView);
        var post = await _mediator.Send(query, cancellationToken);
        if (post == null)
        {
            return NotFound();
        }
        return Ok(post);
    }

    /// <summary>
    /// Öne çıkan blog post'ları getirir
    /// </summary>
    /// <param name="count">Getirilecek post sayısı (varsayılan: 5)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Öne çıkan blog post listesi</returns>
    /// <response code="200">Öne çıkan blog post'ları başarıyla getirildi</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("posts/featured")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<BlogPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<BlogPostDto>>> GetFeaturedPosts(
        [FromQuery] int count = 5,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetFeaturedBlogPostsQuery(count);
        var posts = await _mediator.Send(query, cancellationToken);
        return Ok(posts);
    }

    /// <summary>
    /// Son blog post'ları getirir
    /// </summary>
    /// <param name="count">Getirilecek post sayısı (varsayılan: 10)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Son blog post listesi</returns>
    /// <response code="200">Son blog post'ları başarıyla getirildi</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("posts/recent")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<BlogPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<BlogPostDto>>> GetRecentPosts(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetRecentBlogPostsQuery(count);
        var posts = await _mediator.Send(query, cancellationToken);
        return Ok(posts);
    }

    /// <summary>
    /// Blog post'larında arama yapar (sayfalanmış)
    /// </summary>
    /// <param name="query">Arama sorgusu</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış arama sonuçları</returns>
    /// <response code="200">Arama sonuçları başarıyla getirildi</response>
    /// <response code="400">Geçersiz arama parametreleri</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("posts/search")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<BlogPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<BlogPostDto>>> SearchPosts(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var searchQuery = new SearchBlogPostsQuery(query, page, pageSize);
        var posts = await _mediator.Send(searchQuery, cancellationToken);
        return Ok(posts);
    }

    /// <summary>
    /// Yeni blog post oluşturur
    /// </summary>
    /// <param name="command">Blog post oluşturma komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan blog post</returns>
    /// <response code="201">Blog post başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("posts")]
    [Authorize(Roles = "Admin,Manager,Writer")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(BlogPostDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BlogPostDto>> CreatePost(
        [FromBody] CreateBlogPostCommand command,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var authorId = GetUserId();
        var createCommand = command with { AuthorId = authorId };
        var post = await _mediator.Send(createCommand, cancellationToken);
        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
    }

    /// <summary>
    /// Blog post'u günceller
    /// </summary>
    /// <param name="id">Güncellenecek blog post ID</param>
    /// <param name="command">Blog post güncelleme komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Blog post başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Blog post bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPut("posts/{id}")]
    [Authorize(Roles = "Admin,Manager,Writer")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdatePost(
        Guid id,
        [FromBody] UpdateBlogPostCommand command,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (UpdateBlogPostCommandHandler)
        // Admin/Manager ise PerformedBy = null (tüm post'ları güncelleyebilir), Writer ise PerformedBy = userId
        var userId = GetUserId();
        var performedBy = User.IsInRole("Admin") || User.IsInRole("Manager") ? (Guid?)null : userId;
        var updateCommand = command with { Id = id, PerformedBy = performedBy };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Blog post'u siler
    /// </summary>
    /// <param name="id">Silinecek blog post ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Blog post başarıyla silindi</response>
    /// <response code="404">Blog post bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete("posts/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeletePost(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (DeleteBlogPostCommandHandler)
        // Admin ise PerformedBy = null (tüm post'ları silebilir), Manager ise PerformedBy = userId
        var userId = GetUserId();
        var performedBy = User.IsInRole("Admin") ? (Guid?)null : userId;
        var command = new DeleteBlogPostCommand(id, performedBy);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Blog post'u yayınlar
    /// </summary>
    /// <param name="id">Yayınlanacak blog post ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Blog post başarıyla yayınlandı</response>
    /// <response code="404">Blog post bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("posts/{id}/publish")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PublishPost(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (PublishBlogPostCommandHandler)
        // Admin ise PerformedBy = null (tüm post'ları yayınlayabilir), Manager ise PerformedBy = userId
        var userId = GetUserId();
        var performedBy = User.IsInRole("Admin") ? (Guid?)null : userId;
        var command = new PublishBlogPostCommand(id, performedBy);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Comments
    /// <summary>
    /// Blog post yorumlarını getirir (sayfalanmış)
    /// </summary>
    /// <param name="postId">Blog post ID</param>
    /// <param name="isApproved">Sadece onaylanmış yorumları getir (varsayılan: true)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış yorum listesi</returns>
    /// <response code="200">Yorumlar başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("posts/{postId}/comments")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<BlogCommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<BlogCommentDto>>> GetPostComments(
        Guid postId,
        [FromQuery] bool? isApproved = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetBlogPostCommentsQuery(postId, isApproved, page, pageSize);
        var comments = await _mediator.Send(query, cancellationToken);
        return Ok(comments);
    }

    /// <summary>
    /// Blog post yorumu oluşturur
    /// </summary>
    /// <param name="command">Blog yorumu oluşturma komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan yorum</returns>
    /// <response code="201">Yorum başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Blog post bulunamadı veya yorumlar kapalı</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("comments")]
    [AllowAnonymous]
    [RateLimit(5, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 5 yorum / saat (spam koruması)
    [ProducesResponseType(typeof(BlogCommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BlogCommentDto>> CreateComment(
        [FromBody] CreateBlogCommentCommand command,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = GetUserId();
        }
        var createCommand = command with { UserId = userId };
        var comment = await _mediator.Send(createCommand, cancellationToken);
        return CreatedAtAction(nameof(GetPostComments), new { postId = comment.BlogPostId }, comment);
    }

    /// <summary>
    /// Blog yorumunu onaylar
    /// </summary>
    /// <param name="id">Onaylanacak yorum ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Yorum başarıyla onaylandı</response>
    /// <response code="404">Yorum bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("comments/{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ApproveComment(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new ApproveBlogCommentCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Blog yorumunu siler
    /// </summary>
    /// <param name="id">Silinecek yorum ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Yorum başarıyla silindi</response>
    /// <response code="404">Yorum bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete("comments/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteComment(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new DeleteBlogCommentCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Analytics
    /// <summary>
    /// Blog analytics'ini getirir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Blog analytics verileri</returns>
    /// <response code="200">Analytics başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("analytics")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika (analytics için yüksek limit)
    [ProducesResponseType(typeof(BlogAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BlogAnalyticsDto>> GetAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetBlogAnalyticsQuery(startDate, endDate);
        var analytics = await _mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }
}

