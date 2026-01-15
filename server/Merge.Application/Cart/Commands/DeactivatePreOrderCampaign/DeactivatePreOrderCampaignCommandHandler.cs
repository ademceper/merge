using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.DeactivatePreOrderCampaign;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeactivatePreOrderCampaignCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork) : IRequestHandler<DeactivatePreOrderCampaignCommand, bool>
{

    public async Task<bool> Handle(DeactivatePreOrderCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await context.Set<Merge.Domain.Modules.Marketing.PreOrderCampaign>()
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (campaign is null) return false;

        campaign.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

