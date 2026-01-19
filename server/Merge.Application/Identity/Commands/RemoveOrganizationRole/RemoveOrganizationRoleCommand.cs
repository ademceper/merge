using MediatR;

namespace Merge.Application.Identity.Commands.RemoveOrganizationRole;

public record RemoveOrganizationRoleCommand(Guid OrganizationRoleId) : IRequest<bool>;
