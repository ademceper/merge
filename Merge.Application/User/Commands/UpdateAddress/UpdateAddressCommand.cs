using MediatR;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Application.User.Commands.UpdateAddress;

public record UpdateAddressCommand(
    Guid Id,
    Guid? UserId,
    bool IsAdminOrManager,
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
