using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Catalog;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Catalog.Queries.GetCategoryById;
using Merge.Application.Catalog.Queries.GetAllCategories;
using Merge.Application.Catalog.Queries.GetMainCategories;
using Merge.Application.Catalog.Commands.CreateCategory;
using Merge.Application.Catalog.Commands.UpdateCategory;
using Merge.Application.Catalog.Commands.PatchCategory;
using Merge.Application.Catalog.Commands.DeleteCategory;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Catalog;

/// <summary>
/// Category API endpoints.
/// Tüm kategori operasyonlarını yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/catalog/categories")]
[Tags("Categories")]
public class CategoriesController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// Tüm kategorileri sayfalanmış olarak getirir
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış kategori listesi</returns>
    /// <response code="200">Kategoriler başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<CategoryDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetAllCategoriesQuery(page, pageSize);
        var categories = await mediator.Send(query, cancellationToken);
        return Ok(categories);
    }

    /// <summary>
    /// Ana kategorileri sayfalanmış olarak getirir
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış ana kategori listesi</returns>
    /// <response code="200">Ana kategoriler başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("main")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<CategoryDto>>> GetMainCategories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetMainCategoriesQuery(page, pageSize);
        var categories = await mediator.Send(query, cancellationToken);
        return Ok(categories);
    }

    /// <summary>
    /// Kategori detaylarını getirir
    /// </summary>
    /// <param name="id">Kategori ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kategori detayları</returns>
    /// <response code="200">Kategori başarıyla getirildi</response>
    /// <response code="404">Kategori bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CategoryDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoryByIdQuery(id);
        var category = await mediator.Send(query, cancellationToken);
        
        if (category is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(category);
    }

    /// <summary>
    /// Yeni kategori oluşturur
    /// </summary>
    /// <param name="command">Kategori oluşturma komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan kategori</returns>
    /// <response code="201">Kategori başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

        var category = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>
    /// Kategoriyi günceller
    /// </summary>
    /// <param name="id">Kategori ID</param>
    /// <param name="command">Kategori güncelleme komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncellenen kategori</returns>
    /// <response code="200">Kategori başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">Kategori bulunamadı</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CategoryDto>> Update(
        Guid id,
        [FromBody] UpdateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

        var updateCommand = command with { Id = id };
        var category = await mediator.Send(updateCommand, cancellationToken);
        return Ok(category);
    }

    /// <summary>
    /// Partially update a category.
    /// Only provided fields will be updated.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CategoryDto>> Patch(
        Guid id,
        [FromBody] PatchCategoryDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;

        var command = new PatchCategoryCommand(id, patchDto);
        var category = await mediator.Send(command, cancellationToken);
        return Ok(category);
    }

    /// <summary>
    /// Kategoriyi siler
    /// </summary>
    /// <param name="id">Kategori ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <response code="204">Kategori başarıyla silindi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">Kategori bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (örneğin alt kategoriler varsa silinemez)</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteCategoryCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }
}

