using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.IncrementFaqViewCount;

public class IncrementFaqViewCountCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<IncrementFaqViewCountCommandHandler> logger) : IRequestHandler<IncrementFaqViewCountCommand, bool>
{

    public async Task<bool> Handle(IncrementFaqViewCountCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Incrementing view count for FAQ {FaqId}", request.FaqId);

        var faq = await context.Set<FAQ>()
            .FirstOrDefaultAsync(f => f.Id == request.FaqId, cancellationToken);

        if (faq == null)
        {
            logger.LogWarning("FAQ {FaqId} not found for view count increment", request.FaqId);
            throw new NotFoundException("FAQ", request.FaqId);
        }

        faq.IncrementViewCount();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("View count incremented for FAQ {FaqId}. New count: {ViewCount}", request.FaqId, faq.ViewCount);

        return true;
    }
}
