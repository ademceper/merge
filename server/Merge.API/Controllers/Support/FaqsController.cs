using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Support.Commands.CreateFaq;
using Merge.Application.Support.Commands.UpdateFaq;
using Merge.Application.Support.Commands.DeleteFaq;
using Merge.Application.Support.Commands.IncrementFaqViewCount;
using Merge.Application.Support.Queries.GetFaq;
using Merge.Application.Support.Queries.GetPublishedFaqs;
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
/// FAQs Controller - Manages frequently asked questions
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/support/faqs")]
public class FaqsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly SupportSettings _settings;

    public FaqsController(
        IMediator mediator,
        IOptions<SupportSettings> settings)
    {
        _mediator = mediator;
        _settings = settings.Value;
    }

    /// <summary>
    /// Gets all published FAQs with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of published FAQs</returns>
    /// <response code="200">FAQs retrieved successfully</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet(Name = "GetPublished")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<FaqDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<FaqDto>>> GetPublished(
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
        var query = new GetPublishedFaqsQuery(null, page, pageSize);
        var faqs = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each FAQ and pagination links
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var updatedItems = faqs.Items.Select(faq =>
        {
            var links = HateoasHelper.CreateFaqLinks(Url, faq.Id, version);
            return faq with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        }).ToList();
        
        // Add pagination links
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url,
            "GetPublished",
            faqs.Page,
            faqs.PageSize,
            faqs.TotalPages,
            null,
            version);
        
        return Ok(faqs);
    }

    /// <summary>
    /// Gets published FAQs by category with pagination
    /// </summary>
    /// <param name="category">FAQ category</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of FAQs in the category</returns>
    /// <response code="200">FAQs retrieved successfully</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("category/{category}", Name = "GetByCategory")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<FaqDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<FaqDto>>> GetByCategory(
        string category,
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
        var query = new GetPublishedFaqsQuery(category, page, pageSize);
        var faqs = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each FAQ and pagination links
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var updatedItems = faqs.Items.Select(faq =>
        {
            var links = HateoasHelper.CreateFaqLinks(Url, faq.Id, version);
            return faq with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        }).ToList();
        
        // Add pagination links
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url,
            "GetByCategory",
            faqs.Page,
            faqs.PageSize,
            faqs.TotalPages,
            new { category },
            version);
        
        return Ok(faqs);
    }

    /// <summary>
    /// Gets all FAQs including unpublished ones (Admin only)
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of all FAQs</returns>
    /// <response code="200">FAQs retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("all", Name = "GetAll")]
    [Authorize(Roles = "Admin")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(PagedResult<FaqDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<FaqDto>>> GetAll(
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
        // Note: GetAll için ayrı bir Query oluşturulabilir, şimdilik GetPublishedFaqs kullanıyoruz
        var query = new GetPublishedFaqsQuery(null, page, pageSize);
        var faqs = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to each FAQ and pagination links
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var updatedItems = faqs.Items.Select(faq =>
        {
            var links = HateoasHelper.CreateFaqLinks(Url, faq.Id, version);
            return faq with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        }).ToList();
        
        // Add pagination links
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url,
            "GetAll",
            faqs.Page,
            faqs.PageSize,
            faqs.TotalPages,
            null,
            version);
        
        return Ok(faqs);
    }

    /// <summary>
    /// Gets a FAQ by ID
    /// </summary>
    /// <param name="id">FAQ ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The FAQ</returns>
    /// <response code="200">FAQ found</response>
    /// <response code="404">FAQ not found</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}", Name = "GetById")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(FaqDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FaqDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetFaqQuery(id);
        var faq = await _mediator.Send(query, cancellationToken);
        if (faq == null)
        {
            return NotFound();
        }
        
        // Görüntülenme sayısını artır
        var incrementCommand = new IncrementFaqViewCountCommand(id);
        await _mediator.Send(incrementCommand, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateFaqLinks(Url, faq.Id, version);
        faq = faq with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return Ok(faq);
    }

    /// <summary>
    /// Creates a new FAQ (Admin only)
    /// </summary>
    /// <param name="dto">FAQ creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created FAQ</returns>
    /// <response code="201">FAQ created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(FaqDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FaqDto>> Create(
        [FromBody] CreateFaqDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new CreateFaqCommand(
            dto.Question,
            dto.Answer,
            dto.Category,
            dto.SortOrder,
            dto.IsPublished);
        var faq = await _mediator.Send(command, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateFaqLinks(Url, faq.Id, version);
        faq = faq with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return CreatedAtAction(nameof(GetById), new { version, id = faq.Id }, faq);
    }

    /// <summary>
    /// Updates an existing FAQ (Admin only)
    /// </summary>
    /// <param name="id">FAQ ID</param>
    /// <param name="dto">FAQ update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated FAQ</returns>
    /// <response code="200">FAQ updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">FAQ not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPut("{id}", Name = "Update")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(FaqDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FaqDto>> Update(
        Guid id,
        [FromBody] UpdateFaqDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new UpdateFaqCommand(
            id,
            dto.Question,
            dto.Answer,
            dto.Category,
            dto.SortOrder,
            dto.IsPublished);
        var faq = await _mediator.Send(command, cancellationToken);
        if (faq == null)
        {
            return NotFound();
        }
        
        // ✅ BOLUM 4.1.3: HATEOAS - Add links to response
        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateFaqLinks(Url, faq.Id, version);
        faq = faq with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return Ok(faq);
    }

    /// <summary>
    /// Deletes an FAQ (Admin only)
    /// </summary>
    /// <param name="id">FAQ ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">FAQ deleted successfully</response>
    /// <response code="404">FAQ not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized</response>
    /// <response code="429">Rate limit exceeded</response>
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpDelete("{id}", Name = "Delete")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new DeleteFaqCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

