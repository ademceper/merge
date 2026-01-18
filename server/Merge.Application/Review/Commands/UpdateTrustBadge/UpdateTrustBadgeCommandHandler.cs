using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.UpdateTrustBadge;

public class UpdateTrustBadgeCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateTrustBadgeCommandHandler> logger) : IRequestHandler<UpdateTrustBadgeCommand, TrustBadgeDto>
{

    public async Task<TrustBadgeDto> Handle(UpdateTrustBadgeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating trust badge. BadgeId: {BadgeId}", request.BadgeId);

        var badge = await context.Set<TrustBadge>()
            .FirstOrDefaultAsync(b => b.Id == request.BadgeId, cancellationToken);

        if (badge is null)
        {
            throw new NotFoundException("Rozet", request.BadgeId);
        }

        if (!string.IsNullOrEmpty(request.Name))
            badge.UpdateName(request.Name);
        if (request.Description is not null)
            badge.UpdateDescription(request.Description);
        if (request.IconUrl is not null)
            badge.UpdateIconUrl(request.IconUrl);
        if (!string.IsNullOrEmpty(request.BadgeType))
            badge.UpdateBadgeType(request.BadgeType);
        if (request.Criteria is not null)
        {
            var criteriaJson = JsonSerializer.Serialize(request.Criteria);
            badge.UpdateCriteria(criteriaJson);
        }
        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
                badge.Activate();
            else
                badge.Deactivate();
        }
        if (request.DisplayOrder.HasValue)
            badge.UpdateDisplayOrder(request.DisplayOrder.Value);
        if (request.Color is not null)
            badge.UpdateColor(request.Color);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Trust badge updated successfully. BadgeId: {BadgeId}", request.BadgeId);

        return mapper.Map<TrustBadgeDto>(badge);
    }
}
