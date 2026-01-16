using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Seller;
using Merge.API.Middleware;
using Merge.API.Helpers;
using Merge.Application.Common;
using Merge.Application.Seller.Queries.GetStore;
using Merge.Application.Seller.Queries.GetStoreBySlug;
using Merge.Application.Seller.Queries.GetSellerStores;
using Merge.Application.Seller.Queries.GetPrimaryStore;
using Merge.Application.Seller.Queries.GetStoreStats;
using Merge.Application.Seller.Commands.CreateStore;
using Merge.Application.Seller.Commands.UpdateStore;
using Merge.Application.Seller.Commands.DeleteStore;
using Merge.Application.Seller.Commands.SetPrimaryStore;
using Merge.Application.Seller.Commands.VerifyStore;
using Merge.Application.Seller.Commands.SuspendStore;
using Merge.Domain.Enums;

namespace Merge.API.Controllers.Seller;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/seller/stores")]
public class StoresController(IMediator mediator) : BaseController
{

    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StoreDto>> GetStore(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStoreQuery(id);
        var store = await mediator.Send(query, cancellationToken);

        if (store == null)
        {
            return NotFound();
        }
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetStore", new { version, id }, version);
        links["stats"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{id}/stats", Method = "GET" };
        if (User.Identity?.IsAuthenticated == true && (User.IsInRole("Seller") || User.IsInRole("Admin")))
        {
            links["update"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{id}", Method = "PUT" };
            links["delete"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{id}", Method = "DELETE" };
        }
        return Ok(new { store, _links = links });
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StoreDto>> GetStoreBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStoreBySlugQuery(slug);
        var store = await mediator.Send(query, cancellationToken);

        if (store == null)
        {
            return NotFound();
        }
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetStoreBySlug", new { version, slug }, version);
        links["store"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{store.Id}", Method = "GET" };
        links["stats"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{store.Id}/stats", Method = "GET" };
        return Ok(new { store, _links = links });
    }

    [HttpGet("seller/{sellerId}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<StoreDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<StoreDto>>> GetSellerStores(
        Guid sellerId,
        [FromQuery] EntityStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSellerStoresQuery(sellerId, status, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreatePaginationLinks(Url, "GetSellerStores", page, pageSize, result.TotalPages, new { version, sellerId, status }, version);
        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize, result.TotalPages, _links = links });
    }

    [HttpGet("seller/{sellerId}/primary")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(StoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StoreDto>> GetPrimaryStore(
        Guid sellerId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPrimaryStoreQuery(sellerId);
        var store = await mediator.Send(query, cancellationToken);

        if (store == null)
        {
            return NotFound();
        }
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetPrimaryStore", new { version, sellerId }, version);
        links["store"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{store.Id}", Method = "GET" };
        return Ok(new { store, _links = links });
    }

    [HttpGet("{id}/stats")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(StoreStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StoreStatsDto>> GetStoreStats(
        Guid id,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStoreStatsQuery(id, startDate, endDate);
        var stats = await mediator.Send(query, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetStoreStats", new { version, id, startDate, endDate }, version);
        links["store"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{id}", Method = "GET" };
        return Ok(new { stats, _links = links });
    }

    [HttpPost]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(StoreDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StoreDto>> CreateStore(
        [FromBody] CreateStoreDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var sellerId = GetUserId();
        var command = new CreateStoreCommand(sellerId, dto);
        var store = await mediator.Send(command, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetStore", new { version, id = store.Id }, version);
        links["update"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{store.Id}", Method = "PUT" };
        links["delete"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{store.Id}", Method = "DELETE" };
        links["stats"] = new LinkDto { Href = $"/api/v{version}/seller/stores/{store.Id}/stats", Method = "GET" };
        return CreatedAtAction(nameof(GetStore), new { version, id = store.Id }, new { store, _links = links });
    }

    [HttpGet("my-stores")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<StoreDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<StoreDto>>> GetMyStores(
        [FromQuery] EntityStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var query = new GetSellerStoresQuery(sellerId, status, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreatePaginationLinks(Url, "GetMyStores", page, pageSize, result.TotalPages, new { version, status }, version);
        links["create"] = new LinkDto { Href = $"/api/v{version}/seller/stores", Method = "POST" };
        return Ok(new { result.Items, result.TotalCount, result.Page, result.PageSize, result.TotalPages, _links = links });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateStore(
        Guid id,
        [FromBody] UpdateStoreDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var sellerId = GetUserId();
        var getQuery = new GetStoreQuery(id);
        var store = await mediator.Send(getQuery, cancellationToken);

        if (store == null)
        {
            return NotFound();
        }
        if (store.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var command = new UpdateStoreCommand(id, dto);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Mağazayı kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PatchStore(
        Guid id,
        [FromBody] PatchStoreDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var sellerId = GetUserId();
        var getQuery = new GetStoreQuery(id);
        var store = await mediator.Send(getQuery, cancellationToken);

        if (store == null)
        {
            return NotFound();
        }
        if (store.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var command = new UpdateStoreCommand(id, new UpdateStoreDto
        {
            StoreName = patchDto.StoreName,
            Description = patchDto.Description,
            LogoUrl = patchDto.LogoUrl,
            BannerUrl = patchDto.BannerUrl,
            ContactEmail = patchDto.ContactEmail,
            ContactPhone = patchDto.ContactPhone,
            Address = patchDto.Address,
            City = patchDto.City,
            Country = patchDto.Country,
            PostalCode = patchDto.PostalCode,
            Status = patchDto.Status,
            IsPrimary = patchDto.IsPrimary,
            Settings = patchDto.Settings
        });
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteStore(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var getQuery = new GetStoreQuery(id);
        var store = await mediator.Send(getQuery, cancellationToken);

        if (store == null)
        {
            return NotFound();
        }
        if (store.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var command = new DeleteStoreCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/set-primary")]
    [Authorize(Roles = "Seller,Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SetPrimaryStore(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var sellerId = GetUserId();
        var getQuery = new GetStoreQuery(id);
        var store = await mediator.Send(getQuery, cancellationToken);

        if (store == null)
        {
            return NotFound();
        }
        if (store.SellerId != sellerId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        var command = new SetPrimaryStoreCommand(sellerId, id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/verify")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> VerifyStore(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new VerifyStoreCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/suspend")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SuspendStore(
        Guid id,
        [FromBody] SuspendStoreDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var command = new SuspendStoreCommand(id, dto.Reason);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}
