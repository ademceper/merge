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

public class EvaluateAndAwardBadgesCommandHandler(IDbContext context, IMediator mediator, ILogger<EvaluateAndAwardBadgesCommandHandler> logger) : IRequestHandler<EvaluateAndAwardBadgesCommand>
{

    public async Task Handle(EvaluateAndAwardBadgesCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Evaluating and awarding badges. SellerId: {SellerId}",
            request.SellerId);

        if (request.SellerId.HasValue)
        {
            var evaluateSellerCommand = new EvaluateSellerBadgesCommand(request.SellerId.Value);
            await mediator.Send(evaluateSellerCommand, cancellationToken);
        }
        else
        {
            var sellers = await context.Set<SellerProfile>()
                .AsNoTracking()
                .Where(sp => sp.Status == SellerStatus.Approved)
                .Select(sp => sp.UserId)
                .ToListAsync(cancellationToken);

            logger.LogInformation(
                "Evaluating badges for {Count} sellers",
                sellers.Count);

            foreach (var seller in sellers)
            {
                var evaluateSellerCommand = new EvaluateSellerBadgesCommand(seller);
                await mediator.Send(evaluateSellerCommand, cancellationToken);
            }
        }

        logger.LogInformation(
            "Badge evaluation completed. SellerId: {SellerId}",
            request.SellerId);
    }
}
