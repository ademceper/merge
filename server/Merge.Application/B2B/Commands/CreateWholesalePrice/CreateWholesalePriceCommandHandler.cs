using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using AutoMapper;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using OrganizationEntity = Merge.Domain.Modules.Identity.Organization;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.CreateWholesalePrice;

public class CreateWholesalePriceCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateWholesalePriceCommandHandler> logger) : IRequestHandler<CreateWholesalePriceCommand, WholesalePriceDto>
{

    public async Task<WholesalePriceDto> Handle(CreateWholesalePriceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating wholesale price. ProductId: {ProductId}, OrganizationId: {OrganizationId}",
            request.Dto.ProductId, request.Dto.OrganizationId);


        var product = await context.Set<ProductEntity>()
            .FirstOrDefaultAsync(p => p.Id == request.Dto.ProductId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", request.Dto.ProductId);
        }

        OrganizationEntity? organization = null;
        if (request.Dto.OrganizationId.HasValue)
        {
            organization = await context.Set<OrganizationEntity>()
                .FirstOrDefaultAsync(o => o.Id == request.Dto.OrganizationId.Value, cancellationToken);
        }

        var wholesalePrice = WholesalePrice.Create(
            request.Dto.ProductId,
            product,
            request.Dto.OrganizationId,
            organization,
            request.Dto.MinQuantity,
            request.Dto.MaxQuantity,
            request.Dto.Price,
            request.Dto.IsActive,
            request.Dto.StartDate,
            request.Dto.EndDate);

        await context.Set<WholesalePrice>().AddAsync(wholesalePrice, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        wholesalePrice = await context.Set<WholesalePrice>()
            .AsNoTracking()
            .Include(wp => wp.Product)
            .Include(wp => wp.Organization)
            .FirstOrDefaultAsync(wp => wp.Id == wholesalePrice.Id, cancellationToken);

        logger.LogInformation("Wholesale price created successfully. WholesalePriceId: {WholesalePriceId}", wholesalePrice!.Id);

        return mapper.Map<WholesalePriceDto>(wholesalePrice);
    }
}

