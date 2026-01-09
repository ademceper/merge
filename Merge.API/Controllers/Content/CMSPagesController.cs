using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Content.Commands.CreateCMSPage;
using Merge.Application.Content.Commands.UpdateCMSPage;
using Merge.Application.Content.Commands.DeleteCMSPage;
using Merge.Application.Content.Commands.PublishCMSPage;
using Merge.Application.Content.Commands.SetHomePageCMSPage;
using Merge.Application.Content.Queries.GetCMSPageById;
using Merge.Application.Content.Queries.GetCMSPageBySlug;
using Merge.Application.Content.Queries.GetHomePageCMSPage;
using Merge.Application.Content.Queries.GetAllCMSPages;
using Merge.Application.Content.Queries.GetMenuCMSPages;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Content;

// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/content/cms-pages")]
public class CMSPagesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly PaginationSettings _paginationSettings;

    public CMSPagesController(
        IMediator mediator,
        IOptions<PaginationSettings> paginationSettings)
    {
        _mediator = mediator;
        _paginationSettings = paginationSettings.Value;
    }

    /// <summary>
    /// Yeni CMS sayfası oluşturur
    /// </summary>
    /// <param name="command">CMS sayfası oluşturma komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan CMS sayfası</returns>
    /// <response code="201">CMS sayfası başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(CMSPageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CMSPageDto>> CreatePage(
        [FromBody] CreateCMSPageCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var authorId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var createCommand = command with { AuthorId = authorId };
        var page = await _mediator.Send(createCommand, cancellationToken);
        return CreatedAtAction(nameof(GetPageById), new { id = page.Id }, page);
    }

    /// <summary>
    /// CMS sayfası detaylarını getirir
    /// </summary>
    /// <param name="id">CMS sayfası ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>CMS sayfası detayları</returns>
    /// <response code="200">CMS sayfası başarıyla getirildi</response>
    /// <response code="404">CMS sayfası bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(CMSPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CMSPageDto>> GetPageById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetCMSPageByIdQuery(id);
        var page = await _mediator.Send(query, cancellationToken);
        
        if (page == null)
        {
            return NotFound();
        }
        return Ok(page);
    }

    /// <summary>
    /// Slug'a göre CMS sayfası getirir
    /// </summary>
    /// <param name="slug">CMS sayfası slug</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>CMS sayfası detayları</returns>
    /// <response code="200">CMS sayfası başarıyla getirildi</response>
    /// <response code="404">CMS sayfası bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(CMSPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CMSPageDto>> GetPageBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetCMSPageBySlugQuery(slug);
        var page = await _mediator.Send(query, cancellationToken);
        
        if (page == null)
        {
            return NotFound();
        }
        return Ok(page);
    }

    /// <summary>
    /// Ana sayfa CMS içeriğini getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Ana sayfa CMS içeriği</returns>
    /// <response code="200">Ana sayfa başarıyla getirildi</response>
    /// <response code="404">Ana sayfa bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("home")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(CMSPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CMSPageDto>> GetHomePage(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetHomePageCMSPageQuery();
        var page = await _mediator.Send(query, cancellationToken);
        
        if (page == null)
        {
            return NotFound();
        }
        return Ok(page);
    }

    /// <summary>
    /// Tüm CMS sayfalarını getirir (sayfalanmış)
    /// </summary>
    /// <param name="status">Sayfa durumu (Draft, Published, Archived)</param>
    /// <param name="showInMenu">Menüde gösterilme durumu</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış CMS sayfaları listesi</returns>
    /// <response code="200">CMS sayfaları başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<CMSPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<CMSPageDto>>> GetAllPages(
        [FromQuery] string? status = null,
        [FromQuery] bool? showInMenu = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Config'den al
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetAllCMSPagesQuery(status, showInMenu, page, pageSize);
        var pages = await _mediator.Send(query, cancellationToken);
        return Ok(pages);
    }

    /// <summary>
    /// Menüde gösterilecek CMS sayfalarını getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Menüde gösterilecek CMS sayfaları listesi</returns>
    /// <response code="200">Menü sayfaları başarıyla getirildi</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("menu")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<CMSPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<CMSPageDto>>> GetMenuPages(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ⚠️ NOT: GetMenuCMSPagesQuery handler'da unbounded query koruması var (max 100 sayfa)
        var query = new GetMenuCMSPagesQuery();
        var pages = await _mediator.Send(query, cancellationToken);
        return Ok(pages);
    }

    /// <summary>
    /// CMS sayfasını günceller
    /// </summary>
    /// <param name="id">CMS sayfası ID</param>
    /// <param name="command">CMS sayfası güncelleme komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncelleme işlemi sonucu</returns>
    /// <response code="204">CMS sayfası başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">CMS sayfası bulunamadı</response>
    /// <response code="422">İş kuralı ihlali veya concurrency conflict</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdatePage(
        Guid id,
        [FromBody] UpdateCMSPageCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (UpdateCMSPageCommandHandler)
        // Admin ise PerformedBy = null (tüm sayfaları güncelleyebilir), Manager ise PerformedBy = userId
        Guid? performedBy = User.IsInRole("Admin") ? null : userId;
        var updateCommand = command with { Id = id, PerformedBy = performedBy };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// CMS sayfasını siler
    /// </summary>
    /// <param name="id">CMS sayfası ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <response code="204">CMS sayfası başarıyla silindi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">CMS sayfası bulunamadı</response>
    /// <response code="422">İş kuralı ihlali veya concurrency conflict</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeletePage(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (DeleteCMSPageCommandHandler)
        // Admin ise PerformedBy = null (tüm sayfaları silebilir), Manager ise PerformedBy = userId
        Guid? performedBy = User.IsInRole("Admin") ? null : userId;
        var command = new DeleteCMSPageCommand(id, performedBy);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// CMS sayfasını yayınlar
    /// </summary>
    /// <param name="id">CMS sayfası ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Yayınlama işlemi sonucu</returns>
    /// <response code="204">CMS sayfası başarıyla yayınlandı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">CMS sayfası bulunamadı</response>
    /// <response code="422">İş kuralı ihlali veya concurrency conflict</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PublishPage(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (PublishCMSPageCommandHandler)
        // Admin ise PerformedBy = null (tüm sayfaları yayınlayabilir), Manager ise PerformedBy = userId
        Guid? performedBy = User.IsInRole("Admin") ? null : userId;
        var command = new PublishCMSPageCommand(id, performedBy);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// CMS sayfasını ana sayfa olarak ayarlar
    /// </summary>
    /// <param name="id">CMS sayfası ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Ana sayfa ayarlama işlemi sonucu</returns>
    /// <response code="204">CMS sayfası başarıyla ana sayfa olarak ayarlandı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">CMS sayfası bulunamadı</response>
    /// <response code="422">İş kuralı ihlali veya concurrency conflict</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("{id}/set-home")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SetHomePage(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (SetHomePageCMSPageCommandHandler)
        // Admin ise PerformedBy = null (tüm sayfaları ana sayfa yapabilir), Manager ise PerformedBy = userId
        Guid? performedBy = User.IsInRole("Admin") ? null : userId;
        var command = new SetHomePageCMSPageCommand(id, performedBy);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

