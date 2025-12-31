using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Governance;
using Merge.Application.DTOs.Governance;


namespace Merge.API.Controllers.Governance;

[ApiController]
[Route("api/governance/policies")]
public class PoliciesController : BaseController
{
    private readonly IPolicyService _policyService;

    public PoliciesController(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    // Public endpoints
    [HttpGet("active/{policyType}")]
    public async Task<ActionResult<PolicyDto>> GetActivePolicy(string policyType, [FromQuery] string language = "tr")
    {
        var policy = await _policyService.GetActivePolicyAsync(policyType, language);
        if (policy == null)
        {
            return NotFound();
        }
        return Ok(policy);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PolicyDto>>> GetPolicies(
        [FromQuery] string? policyType = null,
        [FromQuery] string? language = null,
        [FromQuery] bool activeOnly = false)
    {
        var policies = await _policyService.GetPoliciesAsync(policyType, language, activeOnly);
        return Ok(policies);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PolicyDto>> GetPolicy(Guid id)
    {
        var policy = await _policyService.GetPolicyAsync(id);
        if (policy == null)
        {
            return NotFound();
        }
        return Ok(policy);
    }

    [HttpPost("accept")]
    [Authorize]
    public async Task<ActionResult<PolicyAcceptanceDto>> AcceptPolicy([FromBody] AcceptPolicyDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var acceptance = await _policyService.AcceptPolicyAsync(userId, dto, ipAddress);
        return CreatedAtAction(nameof(GetMyAcceptances), new { }, acceptance);
    }

    [HttpGet("my-acceptances")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PolicyAcceptanceDto>>> GetMyAcceptances()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var acceptances = await _policyService.GetUserAcceptancesAsync(userId);
        return Ok(acceptances);
    }

    [HttpGet("pending")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PolicyDto>>> GetPendingPolicies()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var policies = await _policyService.GetPendingPoliciesAsync(userId);
        return Ok(policies);
    }

    [HttpPost("revoke/{policyId}")]
    [Authorize]
    public async Task<IActionResult> RevokeAcceptance(Guid policyId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var success = await _policyService.RevokeAcceptanceAsync(userId, policyId);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Admin endpoints
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PolicyDto>> CreatePolicy([FromBody] CreatePolicyDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var policy = await _policyService.CreatePolicyAsync(dto, userId);
        return CreatedAtAction(nameof(GetPolicy), new { id = policy.Id }, policy);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PolicyDto>> UpdatePolicy(Guid id, [FromBody] UpdatePolicyDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var policy = await _policyService.UpdatePolicyAsync(id, dto, userId);
        if (policy == null)
        {
            return NotFound();
        }
        return Ok(policy);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeletePolicy(Guid id)
    {
        var success = await _policyService.DeletePolicyAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/activate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ActivatePolicy(Guid id)
    {
        var success = await _policyService.ActivatePolicyAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeactivatePolicy(Guid id)
    {
        var success = await _policyService.DeactivatePolicyAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("stats/acceptances")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<Dictionary<string, int>>> GetAcceptanceStats()
    {
        var stats = await _policyService.GetAcceptanceStatsAsync();
        return Ok(stats);
    }

    [HttpGet("{id}/acceptances/count")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<int>> GetAcceptanceCount(Guid id)
    {
        var count = await _policyService.GetAcceptanceCountAsync(id);
        return Ok(new { count });
    }
}

