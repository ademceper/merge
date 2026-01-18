using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.DeleteB2BUser;

public class DeleteB2BUserCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteB2BUserCommandHandler> logger) : IRequestHandler<DeleteB2BUserCommand, bool>
{

    public async Task<bool> Handle(DeleteB2BUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting B2B user. B2BUserId: {B2BUserId}", request.Id);

        var b2bUser = await context.Set<B2BUser>()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (b2bUser is null)
        {
            logger.LogWarning("B2B user not found with Id: {B2BUserId}", request.Id);
            return false;
        }

        b2bUser.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("B2B user deleted successfully. B2BUserId: {B2BUserId}", request.Id);
        return true;
    }
}

