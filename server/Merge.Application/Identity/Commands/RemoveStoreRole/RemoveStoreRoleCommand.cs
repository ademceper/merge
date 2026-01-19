using MediatR;

namespace Merge.Application.Identity.Commands.RemoveStoreRole;

public record RemoveStoreRoleCommand(Guid StoreRoleId) : IRequest<bool>;
