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

namespace Merge.Application.Marketing.Commands.CreateFlashSale;

public class CreateFlashSaleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateFlashSaleCommandHandler> logger) : IRequestHandler<CreateFlashSaleCommand, FlashSaleDto>
{
    public async Task<FlashSaleDto> Handle(CreateFlashSaleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating flash sale. Title: {Title}", request.Title);

        var flashSale = FlashSale.Create(
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.BannerImageUrl);

        await context.Set<FlashSale>().AddAsync(flashSale, cancellationToken);
        
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

 
        var createdFlashSale = await context.Set<FlashSale>()
            .AsNoTracking()
            .Include(fs => fs.FlashSaleProducts)
                .ThenInclude(fsp => fsp.Product)
            .FirstOrDefaultAsync(fs => fs.Id == flashSale.Id, cancellationToken);

        if (createdFlashSale == null)
        {
            logger.LogWarning("FlashSale not found after creation. FlashSaleId: {FlashSaleId}", flashSale.Id);
            throw new NotFoundException("Flash Sale", flashSale.Id);
        }

        logger.LogInformation("FlashSale created successfully. FlashSaleId: {FlashSaleId}, Title: {Title}", flashSale.Id, request.Title);

        return mapper.Map<FlashSaleDto>(createdFlashSale);
    }
}
