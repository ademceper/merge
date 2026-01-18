using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.Payment;
using Merge.Application.Payment.Commands.CreatePaymentMethod;
using Merge.Application.Payment.Commands.UpdatePaymentMethod;
using Merge.Application.Payment.Commands.DeletePaymentMethod;
using Merge.Application.Payment.Commands.SetDefaultPaymentMethod;
using Merge.Application.Payment.Queries.GetPaymentMethodById;
using Merge.Application.Payment.Queries.GetPaymentMethodByCode;
using Merge.Application.Payment.Queries.GetAllPaymentMethods;
using Merge.Application.Payment.Queries.GetAvailablePaymentMethods;
using Merge.Application.Payment.Queries.CalculateProcessingFee;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Payment;

/// <summary>
/// Payment Methods API endpoints.
/// Ödeme yöntemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/payments/methods")]
[Tags("PaymentMethods")]
public class PaymentMethodsController(IMediator mediator) : BaseController
{
    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<PaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAllPaymentMethods(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllPaymentMethodsQuery(isActive);
        var methods = await mediator.Send(query, cancellationToken);
        return Ok(methods);
    }

    [HttpGet("available")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<PaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAvailablePaymentMethods(
        [FromQuery] decimal orderAmount,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAvailablePaymentMethodsQuery(orderAmount);
        var methods = await mediator.Send(query, cancellationToken);
        return Ok(methods);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PaymentMethodDto>> GetPaymentMethod(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetPaymentMethodByIdQuery(id);
        var method = await mediator.Send(query, cancellationToken);
        if (method is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(method);
    }

    [HttpGet("code/{code}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PaymentMethodDto>> GetPaymentMethodByCode(string code, CancellationToken cancellationToken = default)
    {
        var query = new GetPaymentMethodByCodeQuery(code);
        var method = await mediator.Send(query, cancellationToken);
        if (method is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(method);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PaymentMethodDto>> CreatePaymentMethod(
        [FromBody] CreatePaymentMethodDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreatePaymentMethodCommand(
            dto.Name,
            dto.Code,
            dto.Description,
            dto.IconUrl,
            dto.IsActive,
            dto.RequiresOnlinePayment,
            dto.RequiresManualVerification,
            dto.MinimumAmount,
            dto.MaximumAmount,
            dto.ProcessingFee,
            dto.ProcessingFeePercentage,
            dto.Settings,
            dto.DisplayOrder,
            dto.IsDefault);
        var method = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPaymentMethod), new { id = method.Id }, method);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdatePaymentMethod(
        Guid id,
        [FromBody] UpdatePaymentMethodDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdatePaymentMethodCommand(
            id,
            dto.Name,
            dto.Description,
            dto.IconUrl,
            dto.IsActive,
            dto.RequiresOnlinePayment,
            dto.RequiresManualVerification,
            dto.MinimumAmount,
            dto.MaximumAmount,
            dto.ProcessingFee,
            dto.ProcessingFeePercentage,
            dto.Settings,
            dto.DisplayOrder,
            dto.IsDefault);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    /// <summary>
    /// Ödeme yöntemini kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PatchPaymentMethod(
        Guid id,
        [FromBody] PatchPaymentMethodDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdatePaymentMethodCommand(
            id,
            patchDto.Name,
            patchDto.Description,
            patchDto.IconUrl,
            patchDto.IsActive,
            patchDto.RequiresOnlinePayment,
            patchDto.RequiresManualVerification,
            patchDto.MinimumAmount,
            patchDto.MaximumAmount,
            patchDto.ProcessingFee,
            patchDto.ProcessingFeePercentage,
            patchDto.Settings,
            patchDto.DisplayOrder,
            patchDto.IsDefault);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeletePaymentMethod(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new DeletePaymentMethodCommand(id);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    [HttpPost("{id}/set-default")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SetDefaultPaymentMethod(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new SetDefaultPaymentMethodCommand(id);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    /// <summary>
    /// Ödeme yöntemi için işlem ücretini hesaplar
    /// </summary>
    /// <param name="id">Ödeme yöntemi ID</param>
    /// <param name="amount">Tutar</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem ücreti</returns>
    /// <response code="200">Ücret başarıyla hesaplandı</response>
    /// <response code="400">Geçersiz parametreler</response>
    /// <response code="404">Ödeme yöntemi bulunamadı</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpGet("{id}/calculate-fee")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> CalculateProcessingFee(
        Guid id,
        [FromQuery] decimal amount,
        CancellationToken cancellationToken = default)
    {
        var query = new CalculateProcessingFeeQuery(id, amount);
        var fee = await mediator.Send(query, cancellationToken);
        return Ok(fee);
    }
}
