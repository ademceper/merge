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

namespace Merge.Application.Order.Commands.RejectReturnRequest;

public class RejectReturnRequestCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<RejectReturnRequestCommandHandler> logger) : IRequestHandler<RejectReturnRequestCommand, bool>
{

    public async Task<bool> Handle(RejectReturnRequestCommand request, CancellationToken cancellationToken)
    {
        var returnRequest = await context.Set<ReturnRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.ReturnRequestId, cancellationToken);

        if (returnRequest == null)
        {
            return false;
        }

        returnRequest.Reject(request.Reason);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Return request rejected. ReturnRequestId: {ReturnRequestId}, Reason: {Reason}", 
            request.ReturnRequestId, request.Reason);

        return true;
    }
}
