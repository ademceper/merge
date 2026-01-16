using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Content;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.API.Middleware;
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
using Merge.Application.Content.Queries.GetBlogAnalytics;

namespace Merge.API.Controllers.Content.BlogPosts;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/content/blog/posts")]
public class BlogPostsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

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
    [HttpGet("")]
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
        var posts = await mediator.Send(query, cancellationToken);
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
    [HttpGet("/{id}")]
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
        var post = await mediator.Send(query, cancellationToken);
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
    [HttpGet("/slug/{slug}")]
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
        var post = await mediator.Send(query, cancellationToken);
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
    [HttpGet("/featured")]
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
        var posts = await mediator.Send(query, cancellationToken);
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
    [HttpGet("/recent")]
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
        var posts = await mediator.Send(query, cancellationToken);
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
    [HttpGet("/search")]
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
        var posts = await mediator.Send(searchQuery, cancellationToken);
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
    [HttpPost("")]
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
        var post = await mediator.Send(createCommand, cancellationToken);
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
    [HttpPut("/{id}")]
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
        var result = await mediator.Send(updateCommand, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Blog post'u kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("/{id}")]
    [Authorize(Roles = "Admin,Manager,Writer")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PatchPost(
        Guid id,
        [FromBody] PatchBlogPostDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var performedBy = User.IsInRole("Admin") || User.IsInRole("Manager") ? (Guid?)null : userId;
        var command = new UpdateBlogPostCommand(
            Id: id,
            CategoryId: patchDto.CategoryId,
            Title: patchDto.Title,
            Excerpt: patchDto.Excerpt,
            Content: patchDto.Content,
            FeaturedImageUrl: patchDto.FeaturedImageUrl,
            Status: patchDto.Status,
            Tags: patchDto.Tags,
            IsFeatured: patchDto.IsFeatured,
            AllowComments: patchDto.AllowComments,
            MetaTitle: patchDto.MetaTitle,
            MetaDescription: patchDto.MetaDescription,
            MetaKeywords: patchDto.MetaKeywords,
            OgImageUrl: patchDto.OgImageUrl,
            PerformedBy: performedBy);
        var result = await mediator.Send(command, cancellationToken);
        
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
    [HttpDelete("/{id}")]
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
        var result = await mediator.Send(command, cancellationToken);
        
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
    [HttpPost("/{id}/publish")]
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
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }


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
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }
}