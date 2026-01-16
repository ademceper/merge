using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.UpdateFlashSale;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class UpdateFlashSaleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<UpdateFlashSaleCommandHandler> logger) : IRequestHandler<UpdateFlashSaleCommand, FlashSaleDto>
{
    public async Task<FlashSaleDto> Handle(UpdateFlashSaleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating flash sale. FlashSaleId: {FlashSaleId}", request.Id);

        var flashSale = await context.Set<FlashSale>()
            .FirstOrDefaultAsync(fs => fs.Id == request.Id, cancellationToken);

        if (flashSale == null)
        {
            logger.LogWarning("FlashSale not found. FlashSaleId: {FlashSaleId}", request.Id);
            throw new NotFoundException("Flash Sale", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        flashSale.UpdateDetails(
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.BannerImageUrl);

        if (request.IsActive)
        {
            flashSale.Activate();
        }
        else
        {
            flashSale.Deactivate();
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedFlashSale = await context.Set<FlashSale>()
            .AsNoTracking()
            .Include(fs => fs.FlashSaleProducts)
                .ThenInclude(fsp => fsp.Product)
            .FirstOrDefaultAsync(fs => fs.Id == flashSale.Id, cancellationToken);

        if (updatedFlashSale == null)
        {
            logger.LogWarning("FlashSale not found after update. FlashSaleId: {FlashSaleId}", flashSale.Id);
            throw new NotFoundException("Flash Sale", flashSale.Id);
        }

        logger.LogInformation("FlashSale updated successfully. FlashSaleId: {FlashSaleId}", request.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<FlashSaleDto>(updatedFlashSale);
    }
}
