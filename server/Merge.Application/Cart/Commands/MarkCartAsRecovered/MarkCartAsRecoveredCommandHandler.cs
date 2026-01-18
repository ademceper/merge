using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.MarkCartAsRecovered;

public class MarkCartAsRecoveredCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<MarkCartAsRecoveredCommandHandler> logger) : IRequestHandler<MarkCartAsRecoveredCommand>
{

    public async Task Handle(MarkCartAsRecoveredCommand request, CancellationToken cancellationToken)
    {
        var emails = await context.Set<AbandonedCartEmail>()
            .Where(e => e.CartId == request.CartId)
            .ToListAsync(cancellationToken);

        foreach (var email in emails)
        {
            email.MarkAsResultedInPurchase();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

