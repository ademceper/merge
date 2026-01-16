using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Governance;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Governance.Commands.CreatePolicy;
using Merge.Application.Governance.Commands.UpdatePolicy;
using Merge.Application.Governance.Commands.DeletePolicy;
using Merge.Application.Governance.Commands.ActivatePolicy;
using Merge.Application.Governance.Commands.DeactivatePolicy;
using Merge.Application.Governance.Commands.AcceptPolicy;
using Merge.Application.Governance.Commands.RevokeAcceptance;
using Merge.Application.Governance.Queries.GetPolicyById;
using Merge.Application.Governance.Queries.GetActivePolicy;
using Merge.Application.Governance.Queries.GetPolicies;
using Merge.Application.Governance.Queries.GetUserAcceptances;
using Merge.Application.Governance.Queries.GetPendingPolicies;
using Merge.Application.Governance.Queries.GetAcceptanceCount;
using Merge.Application.Governance.Queries.GetAcceptanceStats;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Governance;

[ApiController]
[ApiVersion("1.0")] // ✅ BOLUM 4.1: API Versioning (ZORUNLU)
[Route("api/v{version:apiVersion}/governance/policies")]
public class PoliciesController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    // Public endpoints
    /// <summary>
    /// Aktif policy'yi getirir
    /// </summary>
    [HttpGet("active/{policyType}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PolicyDto>> GetActivePolicy(
        string policyType,
        [FromQuery] string language = "tr",
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetActivePolicyQuery(policyType, language);
        var policy = await mediator.Send(query, cancellationToken);
        if (policy == null)
        {
            return NotFound();
        }
        return Ok(policy);
    }

    /// <summary>
    /// Tüm policy'leri getirir (sayfalanmış)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<PolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PolicyDto>>> GetPolicies(
        [FromQuery] string? policyType = null,
        [FromQuery] string? language = null,
        [FromQuery] bool activeOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetPoliciesQuery(policyType, language, activeOnly, page, pageSize);
        var policies = await mediator.Send(query, cancellationToken);
        return Ok(policies);
    }

    /// <summary>
    /// Policy detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PolicyDto>> GetPolicy(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetPolicyByIdQuery(id);
        var policy = await mediator.Send(query, cancellationToken);
        if (policy == null)
        {
            return NotFound();
        }
        return Ok(policy);
    }

    /// <summary>
    /// Policy'yi kabul eder
    /// </summary>
    [HttpPost("accept")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(PolicyAcceptanceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PolicyAcceptanceDto>> AcceptPolicy(
        [FromBody] AcceptPolicyDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new AcceptPolicyCommand(userId, dto.PolicyId, ipAddress, userAgent);
        var acceptance = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMyAcceptances), new { }, acceptance);
    }

    /// <summary>
    /// Kullanıcının kabul ettiği policy'leri getirir
    /// </summary>
    [HttpGet("my-acceptances")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<PolicyAcceptanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<PolicyAcceptanceDto>>> GetMyAcceptances(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetUserAcceptancesQuery(userId);
        var acceptances = await mediator.Send(query, cancellationToken);
        return Ok(acceptances);
    }

    /// <summary>
    /// Kullanıcının kabul etmediği policy'leri getirir
    /// </summary>
    [HttpGet("pending")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<PolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<PolicyDto>>> GetPendingPolicies(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetPendingPoliciesQuery(userId);
        var policies = await mediator.Send(query, cancellationToken);
        return Ok(policies);
    }

    /// <summary>
    /// Policy kabulünü iptal eder
    /// </summary>
    [HttpPost("revoke/{policyId}")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RevokeAcceptance(
        Guid policyId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR Koruması - Handler'da userId kontrolü var
        var command = new RevokeAcceptanceCommand(userId, policyId);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Admin endpoints
    /// <summary>
    /// Yeni policy oluşturur
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PolicyDto>> CreatePolicy(
        [FromBody] CreatePolicyDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new CreatePolicyCommand(
            userId,
            dto.PolicyType,
            dto.Title,
            dto.Content,
            dto.Version ?? "1.0",
            dto.IsActive,
            dto.RequiresAcceptance,
            dto.EffectiveDate,
            dto.ExpiryDate,
            dto.ChangeLog,
            dto.Language ?? "tr");
        var policy = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPolicy), new { id = policy.Id }, policy);
    }

    /// <summary>
    /// Policy'yi günceller
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PolicyDto>> UpdatePolicy(
        Guid id,
        [FromBody] UpdatePolicyDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new UpdatePolicyCommand(
            id,
            userId,
            dto.Title,
            dto.Content,
            dto.Version,
            dto.IsActive,
            dto.RequiresAcceptance,
            dto.EffectiveDate,
            dto.ExpiryDate,
            dto.ChangeLog,
            userId); // PerformedBy for IDOR protection
        var policy = await mediator.Send(command, cancellationToken);
        return Ok(policy);
    }

    /// <summary>
    /// Politikayı kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PolicyDto>> PatchPolicy(
        Guid id,
        [FromBody] PatchPolicyDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var command = new UpdatePolicyCommand(
            id,
            userId,
            patchDto.Title,
            patchDto.Content,
            patchDto.Version,
            patchDto.IsActive,
            patchDto.RequiresAcceptance,
            patchDto.EffectiveDate,
            patchDto.ExpiryDate,
            patchDto.ChangeLog);
        var policy = await mediator.Send(command, cancellationToken);
        return Ok(policy);
    }

    /// <summary>
    /// Policy'yi siler
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeletePolicy(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new DeletePolicyCommand(id);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Policy'yi aktifleştirir
    /// </summary>
    [HttpPost("{id}/activate")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ActivatePolicy(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new ActivatePolicyCommand(id);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Policy'yi deaktifleştirir
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeactivatePolicy(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new DeactivatePolicyCommand(id);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Policy kabul istatistiklerini getirir
    /// </summary>
    [HttpGet("stats/acceptances")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<Dictionary<string, int>>> GetAcceptanceStats(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetAcceptanceStatsQuery();
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Policy kabul sayısını getirir
    /// </summary>
    [HttpGet("{id}/acceptances/count")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<int>> GetAcceptanceCount(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetAcceptanceCountQuery(id);
        var count = await mediator.Send(query, cancellationToken);
        return Ok(new { count });
    }
}
