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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateVolumeDiscountCommandHandler : IRequestHandler<CreateVolumeDiscountCommand, VolumeDiscountDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateVolumeDiscountCommandHandler> _logger;

    public CreateVolumeDiscountCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateVolumeDiscountCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<VolumeDiscountDto> Handle(CreateVolumeDiscountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating volume discount. ProductId: {ProductId}, CategoryId: {CategoryId}, OrganizationId: {OrganizationId}",
            request.Dto.ProductId, request.Dto.CategoryId, request.Dto.OrganizationId);

        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, handler'da tekrar validation gereksiz

        ProductEntity? product = null;
        if (request.Dto.ProductId.HasValue)
        {
            product = await _context.Set<ProductEntity>()
                .FirstOrDefaultAsync(p => p.Id == request.Dto.ProductId.Value, cancellationToken);
        }

        CategoryEntity? category = null;
        if (request.Dto.CategoryId.HasValue)
        {
            category = await _context.Set<CategoryEntity>()
                .FirstOrDefaultAsync(c => c.Id == request.Dto.CategoryId.Value, cancellationToken);
        }

        OrganizationEntity? organization = null;
        if (request.Dto.OrganizationId.HasValue)
        {
            organization = await _context.Set<OrganizationEntity>()
                .FirstOrDefaultAsync(o => o.Id == request.Dto.OrganizationId.Value, cancellationToken);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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

        await _context.Set<VolumeDiscount>().AddAsync(discount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: Reload with Include for AutoMapper
        discount = await _context.Set<VolumeDiscount>()
            .AsNoTracking()
            .Include(vd => vd.Product)
            .Include(vd => vd.Category)
            .Include(vd => vd.Organization)
            .FirstOrDefaultAsync(vd => vd.Id == discount.Id, cancellationToken);

        _logger.LogInformation("Volume discount created successfully. VolumeDiscountId: {VolumeDiscountId}", discount!.Id);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<VolumeDiscountDto>(discount);
    }
}

