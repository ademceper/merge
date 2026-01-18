using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.UpdateShippingAddress;

public record UpdateShippingAddressCommand(
    Guid Id,
    string? Label,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? AddressLine1,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string? AddressLine2,
    bool? IsDefault,
    bool? IsActive,
    string? Instructions) : IRequest<Unit>;

