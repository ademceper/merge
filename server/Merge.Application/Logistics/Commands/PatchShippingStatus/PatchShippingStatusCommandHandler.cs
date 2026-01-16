using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.PatchShippingStatus;

/// <summary>
/// Handler for PatchShippingStatusCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchShippingStatusCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<PatchShippingStatusCommandHandler> logger) : IRequestHandler<PatchShippingStatusCommand, ShippingDto>
{
    public async Task<ShippingDto> Handle(PatchShippingStatusCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching shipping status. ShippingId: {ShippingId}", request.ShippingId);

        var shipping = await context.Set<Shipping>()
            .FirstOrDefaultAsync(s => s.Id == request.ShippingId, cancellationToken);

        if (shipping == null)
        {
            logger.LogWarning("Shipping not found. ShippingId: {ShippingId}", request.ShippingId);
            throw new NotFoundException("Kargo", request.ShippingId);
        }

        if (request.PatchDto.Status != null)
        {
            if (!Enum.TryParse<ShippingStatus>(request.PatchDto.Status, true, out var statusEnum))
            {
                throw new BusinessException("Geçersiz kargo durumu.");
            }
            shipping.TransitionTo(statusEnum);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Shipping status patched successfully. ShippingId: {ShippingId}", request.ShippingId);

        return mapper.Map<ShippingDto>(shipping);
    }
}
