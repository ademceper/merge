using MediatR;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Queries.GetAddressById;

public record GetAddressByIdQuery(
    Guid Id,
    Guid? UserId = null,
    bool IsAdminOrManager = false
) : IRequest<AddressDto?>;
