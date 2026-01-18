using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Commands.CompleteReturnRequest;

public class CompleteReturnRequestCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<CompleteReturnRequestCommandHandler> logger) : IRequestHandler<CompleteReturnRequestCommand, bool>
{

    public async Task<bool> Handle(CompleteReturnRequestCommand request, CancellationToken cancellationToken)
    {
        var returnRequest = await context.Set<ReturnRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.ReturnRequestId, cancellationToken);

        if (returnRequest == null)
        {
            return false;
        }

        returnRequest.Complete(request.TrackingNumber);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Return request completed. ReturnRequestId: {ReturnRequestId}, TrackingNumber: {TrackingNumber}", 
            request.ReturnRequestId, request.TrackingNumber);

        return true;
    }
}
