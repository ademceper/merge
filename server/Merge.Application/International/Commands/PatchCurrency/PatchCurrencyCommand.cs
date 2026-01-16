using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.PatchCurrency;

/// <summary>
/// PATCH command for partial currency updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCurrencyCommand(
    Guid Id,
    PatchCurrencyDto PatchDto
) : IRequest<CurrencyDto>;
