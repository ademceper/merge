using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Identity.Commands.RemoveOrganizationRole;

public class RemoveOrganizationRoleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RemoveOrganizationRoleCommandHandler> logger) : IRequestHandler<RemoveOrganizationRoleCommand, bool>
{
    public async Task<bool> Handle(RemoveOrganizationRoleCommand request, CancellationToken ct)
    {
        logger.LogInformation("Removing organization role. OrganizationRoleId: {OrganizationRoleId}", request.OrganizationRoleId);

        var organizationRole = await context.Set<OrganizationRole>()
            .FirstOrDefaultAsync(or => or.Id == request.OrganizationRoleId && !or.IsDeleted, ct);

        if (organizationRole is null)
        {
            throw new Application.Exceptions.NotFoundException("OrganizationRole", request.OrganizationRoleId);
        }

        organizationRole.Remove();
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
