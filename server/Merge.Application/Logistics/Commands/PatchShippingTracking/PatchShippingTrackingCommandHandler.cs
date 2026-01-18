using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.PatchShippingTracking;

/// <summary>
/// Handler for PatchShippingTrackingCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchShippingTrackingCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<PatchShippingTrackingCommandHandler> logger) : IRequestHandler<PatchShippingTrackingCommand, ShippingDto>
{
    public async Task<ShippingDto> Handle(PatchShippingTrackingCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching shipping tracking. ShippingId: {ShippingId}", request.ShippingId);

        var shipping = await context.Set<Shipping>()
            .FirstOrDefaultAsync(s => s.Id == request.ShippingId, cancellationToken);

        if (shipping is null)
        {
            logger.LogWarning("Shipping not found. ShippingId: {ShippingId}", request.ShippingId);
            throw new NotFoundException("Kargo", request.ShippingId);
        }

        if (request.PatchDto.TrackingNumber is not null)
        {
            shipping.UpdateTrackingNumber(request.PatchDto.TrackingNumber);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Shipping tracking patched successfully. ShippingId: {ShippingId}", request.ShippingId);

        return mapper.Map<ShippingDto>(shipping);
    }
}
