using MediatR;

namespace Merge.Application.Identity.Commands.RemoveStoreCustomerRole;

public record RemoveStoreCustomerRoleCommand(Guid StoreCustomerRoleId) : IRequest<bool>;
