using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Support.Commands.CreateFaq;
using Merge.Application.Support.Commands.UpdateFaq;
using Merge.Application.Support.Commands.PatchFaq;
using Merge.Application.Support.Commands.DeleteFaq;
using Merge.Application.Support.Commands.IncrementFaqViewCount;
using Merge.Application.Support.Queries.GetFaq;
using Merge.Application.Support.Queries.GetPublishedFaqs;
using Merge.API.Middleware;
using Merge.API.Helpers;

namespace Merge.API.Controllers.Support;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/support/faqs")]
public class FaqsController(IMediator mediator, IOptions<SupportSettings> supportSettings) : BaseController
{
    private readonly SupportSettings _supportSettings = supportSettings.Value;

    [HttpGet(Name = "GetPublished")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(PagedResult<FaqDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<FaqDto>>> GetPublished(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
                                if (pageSize <= 0) pageSize = _supportSettings.DefaultPageSize;
        if (pageSize > _supportSettings.MaxPageSize) pageSize = _supportSettings.MaxPageSize;
        if (page < 1) page = 1;

                var query = new GetPublishedFaqsQuery(null, page, pageSize);
        var faqs = await mediator.Send(query, cancellationToken);
        
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

    [HttpGet("category/{category}", Name = "GetByCategory")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(PagedResult<FaqDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<FaqDto>>> GetByCategory(
        string category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
                                if (pageSize <= 0) pageSize = _supportSettings.DefaultPageSize;
        if (pageSize > _supportSettings.MaxPageSize) pageSize = _supportSettings.MaxPageSize;
        if (page < 1) page = 1;

                var query = new GetPublishedFaqsQuery(category, page, pageSize);
        var faqs = await mediator.Send(query, cancellationToken);
        
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

    [HttpGet("all", Name = "GetAll")]
    [Authorize(Roles = "Admin")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(PagedResult<FaqDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<FaqDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 0,
        CancellationToken cancellationToken = default)
    {
                                if (pageSize <= 0) pageSize = _supportSettings.DefaultPageSize;
        if (pageSize > _supportSettings.MaxPageSize) pageSize = _supportSettings.MaxPageSize;
        if (page < 1) page = 1;

                // Note: GetAll için ayrı bir Query oluşturulabilir, şimdilik GetPublishedFaqs kullanıyoruz
        var query = new GetPublishedFaqsQuery(null, page, pageSize);
        var faqs = await mediator.Send(query, cancellationToken);
        
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

    [HttpGet("{id}", Name = "GetById")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(FaqDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FaqDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
                        var query = new GetFaqQuery(id);
        var faq = await mediator.Send(query, cancellationToken);
        if (faq == null)
        {
            return NotFound();
        }
        
        // Görüntülenme sayısını artır
        var incrementCommand = new IncrementFaqViewCountCommand(id);
        await mediator.Send(incrementCommand, cancellationToken);
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateFaqLinks(Url, faq.Id, version);
        faq = faq with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return Ok(faq);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]     [ProducesResponseType(typeof(FaqDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FaqDto>> Create(
        [FromBody] CreateFaqDto dto,
        CancellationToken cancellationToken = default)
    {
                var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

                var command = new CreateFaqCommand(
            dto.Question,
            dto.Answer,
            dto.Category,
            dto.SortOrder,
            dto.IsPublished);
        var faq = await mediator.Send(command, cancellationToken);
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateFaqLinks(Url, faq.Id, version);
        faq = faq with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return CreatedAtAction(nameof(GetById), new { version, id = faq.Id }, faq);
    }

    [HttpPut("{id}", Name = "Update")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]     [ProducesResponseType(typeof(FaqDto), StatusCodes.Status200OK)]
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
                var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

                var command = new UpdateFaqCommand(
            id,
            dto.Question,
            dto.Answer,
            dto.Category,
            dto.SortOrder,
            dto.IsPublished);
        var faq = await mediator.Send(command, cancellationToken);
        if (faq == null)
        {
            return NotFound();
        }
        
                var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateFaqLinks(Url, faq.Id, version);
        faq = faq with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };
        
        return Ok(faq);
    }

    /// <summary>
    /// FAQ'yi kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(FaqDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<FaqDto>> Patch(
        Guid id,
        [FromBody] PatchFaqDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new PatchFaqCommand(id, patchDto);
        var faq = await mediator.Send(command, cancellationToken);
        if (faq == null)
        {
            return NotFound();
        }

        var version = HttpContext.GetRouteValue("version")?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateFaqLinks(Url, faq.Id, version);
        faq = faq with { Links = links.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value) };

        return Ok(faq);
    }

    [HttpDelete("{id}", Name = "Delete")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]     [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
                        var command = new DeleteFaqCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
