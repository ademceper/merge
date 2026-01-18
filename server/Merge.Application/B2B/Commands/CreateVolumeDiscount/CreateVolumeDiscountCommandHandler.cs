using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using AutoMapper;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using CategoryEntity = Merge.Domain.Modules.Catalog.Category;
using OrganizationEntity = Merge.Domain.Modules.Identity.Organization;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.CreateVolumeDiscount;

public class CreateVolumeDiscountCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateVolumeDiscountCommandHandler> logger) : IRequestHandler<CreateVolumeDiscountCommand, VolumeDiscountDto>
{

    public async Task<VolumeDiscountDto> Handle(CreateVolumeDiscountCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating volume discount. ProductId: {ProductId}, CategoryId: {CategoryId}, OrganizationId: {OrganizationId}",
            request.Dto.ProductId, request.Dto.CategoryId, request.Dto.OrganizationId);


        ProductEntity? product = null;
        if (request.Dto.ProductId.HasValue)
        {
            product = await context.Set<ProductEntity>()
                .FirstOrDefaultAsync(p => p.Id == request.Dto.ProductId.Value, cancellationToken);
        }

        CategoryEntity? category = null;
        if (request.Dto.CategoryId.HasValue)
        {
            category = await context.Set<CategoryEntity>()
                .FirstOrDefaultAsync(c => c.Id == request.Dto.CategoryId.Value, cancellationToken);
        }

        OrganizationEntity? organization = null;
        if (request.Dto.OrganizationId.HasValue)
        {
            organization = await context.Set<OrganizationEntity>()
                .FirstOrDefaultAsync(o => o.Id == request.Dto.OrganizationId.Value, cancellationToken);
        }

        var discount = VolumeDiscount.Create(
            request.Dto.ProductId ?? Guid.Empty,
            product,
            request.Dto.CategoryId,
            category,
            request.Dto.OrganizationId,
            organization,
            request.Dto.MinQuantity,
            request.Dto.MaxQuantity,
            request.Dto.DiscountPercentage,
            request.Dto.FixedDiscountAmount,
            request.Dto.IsActive,
            request.Dto.StartDate,
            request.Dto.EndDate);

        await context.Set<VolumeDiscount>().AddAsync(discount, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        discount = await context.Set<VolumeDiscount>()
            .AsNoTracking()
            .Include(vd => vd.Product)
            .Include(vd => vd.Category)
            .Include(vd => vd.Organization)
            .FirstOrDefaultAsync(vd => vd.Id == discount.Id, cancellationToken);

        logger.LogInformation("Volume discount created successfully. VolumeDiscountId: {VolumeDiscountId}", discount!.Id);

        return mapper.Map<VolumeDiscountDto>(discount);
    }
}

