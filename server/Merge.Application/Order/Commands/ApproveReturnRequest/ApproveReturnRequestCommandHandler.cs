using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Commands.ApproveReturnRequest;

public class ApproveReturnRequestCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ApproveReturnRequestCommandHandler> logger) : IRequestHandler<ApproveReturnRequestCommand, bool>
{

    public async Task<bool> Handle(ApproveReturnRequestCommand request, CancellationToken cancellationToken)
    {
        var returnRequest = await context.Set<ReturnRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.ReturnRequestId, cancellationToken);

        if (returnRequest == null)
        {
            return false;
        }

        returnRequest.Approve();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Return request approved. ReturnRequestId: {ReturnRequestId}", request.ReturnRequestId);

        return true;
    }
}
