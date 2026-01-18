using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.CreateShippingAddress;

public record CreateShippingAddressCommand(
    Guid UserId,
    string Label,
    string FirstName,
    string LastName,
    string Phone,
    string AddressLine1,
    string City,
    string State,
    string PostalCode,
    string Country,
    string? AddressLine2,
    bool IsDefault,
    string? Instructions) : IRequest<ShippingAddressDto>;

