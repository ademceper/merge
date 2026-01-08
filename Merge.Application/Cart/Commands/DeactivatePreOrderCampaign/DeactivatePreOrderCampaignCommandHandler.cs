using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;

namespace Merge.Application.Cart.Commands.DeactivatePreOrderCampaign;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeactivatePreOrderCampaignCommandHandler : IRequestHandler<DeactivatePreOrderCampaignCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivatePreOrderCampaignCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeactivatePreOrderCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _context.Set<Domain.Entities.PreOrderCampaign>()
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        if (campaign == null) return false;

        campaign.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

