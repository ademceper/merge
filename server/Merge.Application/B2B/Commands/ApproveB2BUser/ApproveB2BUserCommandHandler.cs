using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using Merge.Domain.SharedKernel.DomainEvents;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.ApproveB2BUser;

public class ApproveB2BUserCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ApproveB2BUserCommandHandler> logger) : IRequestHandler<ApproveB2BUserCommand, bool>
{

    public async Task<bool> Handle(ApproveB2BUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Approving B2B user. B2BUserId: {B2BUserId}, ApprovedByUserId: {ApprovedByUserId}",
            request.Id, request.ApprovedByUserId);

        var b2bUser = await context.Set<B2BUser>()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (b2bUser is null)
        {
            logger.LogWarning("B2B user not found with Id: {B2BUserId}", request.Id);
            return false;
        }

        b2bUser.Approve(request.ApprovedByUserId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("B2B user approved successfully. B2BUserId: {B2BUserId}", request.Id);
        return true;
    }
}

