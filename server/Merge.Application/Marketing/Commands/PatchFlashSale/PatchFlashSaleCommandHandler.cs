using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.PatchFlashSale;

/// <summary>
/// Handler for PatchFlashSaleCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchFlashSaleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<PatchFlashSaleCommandHandler> logger) : IRequestHandler<PatchFlashSaleCommand, FlashSaleDto>
{
    public async Task<FlashSaleDto> Handle(PatchFlashSaleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching flash sale. FlashSaleId: {FlashSaleId}", request.Id);

        var flashSale = await context.Set<FlashSale>()
            .FirstOrDefaultAsync(fs => fs.Id == request.Id, cancellationToken);

        if (flashSale == null)
        {
            logger.LogWarning("FlashSale not found. FlashSaleId: {FlashSaleId}", request.Id);
            throw new NotFoundException("Flash Sale", request.Id);
        }

        // Apply partial updates - only update fields that are provided
        var title = request.PatchDto.Title ?? flashSale.Title;
        var description = request.PatchDto.Description ?? flashSale.Description;
        var startDate = request.PatchDto.StartDate ?? flashSale.StartDate;
        var endDate = request.PatchDto.EndDate ?? flashSale.EndDate;
        var bannerImageUrl = request.PatchDto.BannerImageUrl ?? flashSale.BannerImageUrl;

        flashSale.UpdateDetails(title, description, startDate, endDate, bannerImageUrl);

        if (request.PatchDto.IsActive.HasValue)
        {
            if (request.PatchDto.IsActive.Value)
            {
                flashSale.Activate();
            }
            else
            {
                flashSale.Deactivate();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedFlashSale = await context.Set<FlashSale>()
            .AsNoTracking()
            .Include(fs => fs.FlashSaleProducts)
                .ThenInclude(fsp => fsp.Product)
            .FirstOrDefaultAsync(fs => fs.Id == flashSale.Id, cancellationToken);

        if (updatedFlashSale == null)
        {
            logger.LogWarning("FlashSale not found after patch. FlashSaleId: {FlashSaleId}", flashSale.Id);
            throw new NotFoundException("Flash Sale", flashSale.Id);
        }

        logger.LogInformation("FlashSale patched successfully. FlashSaleId: {FlashSaleId}", request.Id);

        return mapper.Map<FlashSaleDto>(updatedFlashSale);
    }
}
