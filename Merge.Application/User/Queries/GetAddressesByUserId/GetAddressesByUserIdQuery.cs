using MediatR;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Queries.GetAddressesByUserId;

public record GetAddressesByUserIdQuery(Guid UserId) : IRequest<IEnumerable<AddressDto>>;
