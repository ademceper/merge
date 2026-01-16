using MediatR;
using Merge.Application.DTOs.User;

namespace Merge.Application.User.Commands.PatchAddress;

/// <summary>
/// PATCH command for partial address updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchAddressCommand(
    Guid Id,
    PatchAddressDto PatchDto,
    Guid? UserId = null,
    bool IsAdminOrManager = false
) : IRequest<AddressDto>;
