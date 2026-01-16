using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.PatchFlashSale;

/// <summary>
/// PATCH command for partial flash sale updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchFlashSaleCommand(
    Guid Id,
    PatchFlashSaleDto PatchDto
) : IRequest<FlashSaleDto>;
