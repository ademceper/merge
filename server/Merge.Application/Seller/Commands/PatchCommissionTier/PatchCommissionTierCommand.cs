using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.PatchCommissionTier;

/// <summary>
/// PATCH command for partial commission tier updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCommissionTierCommand(
    Guid TierId,
    PatchCommissionTierDto PatchDto
) : IRequest<bool>;
