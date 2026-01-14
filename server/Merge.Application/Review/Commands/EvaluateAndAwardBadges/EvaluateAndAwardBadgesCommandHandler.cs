using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.Review.Commands.EvaluateSellerBadges;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.EvaluateAndAwardBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class EvaluateAndAwardBadgesCommandHandler : IRequestHandler<EvaluateAndAwardBadgesCommand>
{
    private readonly IDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<EvaluateAndAwardBadgesCommandHandler> _logger;

    public EvaluateAndAwardBadgesCommandHandler(
        IDbContext context,
        IMediator mediator,
        ILogger<EvaluateAndAwardBadgesCommandHandler> logger)
    {
        _context = context;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(EvaluateAndAwardBadgesCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Evaluating and awarding badges. SellerId: {SellerId}",
            request.SellerId);

        if (request.SellerId.HasValue)
        {
            var evaluateSellerCommand = new EvaluateSellerBadgesCommand(request.SellerId.Value);
            await _mediator.Send(evaluateSellerCommand, cancellationToken);
        }
        else
        {
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !sp.IsDeleted (Global Query Filter)
            var sellers = await _context.Set<SellerProfile>()
                .AsNoTracking()
                .Where(sp => sp.Status == SellerStatus.Approved)
                .Select(sp => sp.UserId)
                .ToListAsync(cancellationToken);

            _logger.LogInformation(
                "Evaluating badges for {Count} sellers",
                sellers.Count);

            foreach (var seller in sellers)
            {
                var evaluateSellerCommand = new EvaluateSellerBadgesCommand(seller);
                await _mediator.Send(evaluateSellerCommand, cancellationToken);
            }
        }

        _logger.LogInformation(
            "Badge evaluation completed. SellerId: {SellerId}",
            request.SellerId);
    }
}
