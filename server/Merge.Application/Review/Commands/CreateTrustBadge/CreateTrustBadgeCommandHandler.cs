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

namespace Merge.Application.Review.Commands.CreateTrustBadge;

public class CreateTrustBadgeCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateTrustBadgeCommandHandler> logger) : IRequestHandler<CreateTrustBadgeCommand, TrustBadgeDto>
{

    public async Task<TrustBadgeDto> Handle(CreateTrustBadgeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Creating trust badge. Name: {Name}, BadgeType: {BadgeType}",
            request.Name, request.BadgeType);

        var criteriaJson = request.Criteria is not null ? JsonSerializer.Serialize(request.Criteria) : string.Empty;
        var badge = TrustBadge.Create(
            request.Name,
            request.Description,
            request.IconUrl,
            request.BadgeType,
            criteriaJson,
            request.DisplayOrder,
            request.Color);

        if (!request.IsActive)
        {
            badge.Deactivate();
        }

        await context.Set<TrustBadge>().AddAsync(badge, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Trust badge created successfully. BadgeId: {BadgeId}, Name: {Name}",
            badge.Id, badge.Name);

        return mapper.Map<TrustBadgeDto>(badge);
    }
}
