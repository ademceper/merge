using MediatR;
using Merge.Application.DTOs.User;

namespace Merge.Application.User.Commands.CreateAddress;

public record CreateAddressCommand(
    Guid UserId,
    string Title,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string District,
    string PostalCode,
    string Country,
    bool IsDefault
) : IRequest<AddressDto>;
