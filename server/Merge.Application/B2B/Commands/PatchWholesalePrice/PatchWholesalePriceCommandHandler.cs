using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Payment;
using Merge.Application.B2B.Commands.UpdateWholesalePrice;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.PatchWholesalePrice;

public class PatchWholesalePriceCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<PatchWholesalePriceCommandHandler> logger) : IRequestHandler<PatchWholesalePriceCommand, bool>
{
    public async Task<bool> Handle(PatchWholesalePriceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching wholesale price. WholesalePriceId: {WholesalePriceId}", request.Id);

        var wholesalePrice = await context.Set<WholesalePrice>()
            .FirstOrDefaultAsync(wp => wp.Id == request.Id, cancellationToken);

        if (wholesalePrice == null)
        {
            logger.LogWarning("Wholesale price not found. WholesalePriceId: {WholesalePriceId}", request.Id);
            return false;
        }

        var dto = new CreateWholesalePriceDto
        {
            ProductId = request.PatchDto.ProductId ?? wholesalePrice.ProductId,
            OrganizationId = request.PatchDto.OrganizationId ?? wholesalePrice.OrganizationId,
            MinQuantity = request.PatchDto.MinQuantity ?? wholesalePrice.MinQuantity,
            MaxQuantity = request.PatchDto.MaxQuantity ?? wholesalePrice.MaxQuantity,
            Price = request.PatchDto.Price ?? wholesalePrice.Price,
            IsActive = request.PatchDto.IsActive ?? wholesalePrice.IsActive,
            StartDate = request.PatchDto.StartDate ?? wholesalePrice.StartDate,
            EndDate = request.PatchDto.EndDate ?? wholesalePrice.EndDate
        };

        var updateCommand = new UpdateWholesalePriceCommand(request.Id, dto);

        return await mediator.Send(updateCommand, cancellationToken);
    }
}
