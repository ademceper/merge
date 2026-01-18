using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Commands.VerifyOrder;

public class VerifyOrderCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<VerifyOrderCommandHandler> logger) : IRequestHandler<VerifyOrderCommand, bool>
{

    public async Task<bool> Handle(VerifyOrderCommand request, CancellationToken cancellationToken)
    {
        var verification = await context.Set<OrderVerification>()
            .FirstOrDefaultAsync(v => v.Id == request.VerificationId, cancellationToken);

        if (verification == null) return false;

        verification.Verify(request.VerifiedByUserId, request.Notes);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Order verified. VerificationId: {VerificationId}, VerifiedByUserId: {VerifiedByUserId}",
            request.VerificationId, request.VerifiedByUserId);

        return true;
    }
}
