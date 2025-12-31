using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Payment;
using Merge.Application.DTOs.Payment;


namespace Merge.API.Controllers.Payment;

[ApiController]
[Route("api/payments/methods")]
public class PaymentMethodsController : BaseController
{
    private readonly IPaymentMethodService _paymentMethodService;

    public PaymentMethodsController(IPaymentMethodService paymentMethodService)
    {
        _paymentMethodService = paymentMethodService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAllPaymentMethods([FromQuery] bool? isActive = null)
    {
        var methods = await _paymentMethodService.GetAllPaymentMethodsAsync(isActive);
        return Ok(methods);
    }

    [HttpGet("available")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAvailablePaymentMethods([FromQuery] decimal orderAmount)
    {
        var methods = await _paymentMethodService.GetAvailablePaymentMethodsAsync(orderAmount);
        return Ok(methods);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PaymentMethodDto>> GetPaymentMethod(Guid id)
    {
        var method = await _paymentMethodService.GetPaymentMethodByIdAsync(id);
        if (method == null)
        {
            return NotFound();
        }
        return Ok(method);
    }

    [HttpGet("code/{code}")]
    [AllowAnonymous]
    public async Task<ActionResult<PaymentMethodDto>> GetPaymentMethodByCode(string code)
    {
        var method = await _paymentMethodService.GetPaymentMethodByCodeAsync(code);
        if (method == null)
        {
            return NotFound();
        }
        return Ok(method);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PaymentMethodDto>> CreatePaymentMethod([FromBody] CreatePaymentMethodDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var method = await _paymentMethodService.CreatePaymentMethodAsync(dto);
        return CreatedAtAction(nameof(GetPaymentMethod), new { id = method.Id }, method);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdatePaymentMethod(Guid id, [FromBody] UpdatePaymentMethodDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _paymentMethodService.UpdatePaymentMethodAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeletePaymentMethod(Guid id)
    {
        var success = await _paymentMethodService.DeletePaymentMethodAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/set-default")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SetDefaultPaymentMethod(Guid id)
    {
        var success = await _paymentMethodService.SetDefaultPaymentMethodAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("{id}/calculate-fee")]
    [AllowAnonymous]
    public async Task<ActionResult<decimal>> CalculateProcessingFee(Guid id, [FromQuery] decimal amount)
    {
        var fee = await _paymentMethodService.CalculateProcessingFeeAsync(id, amount);
        return Ok(new { paymentMethodId = id, amount, processingFee = fee });
    }
}

