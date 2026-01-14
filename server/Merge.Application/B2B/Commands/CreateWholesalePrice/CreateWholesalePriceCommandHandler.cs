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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateWholesalePriceCommandHandler : IRequestHandler<CreateWholesalePriceCommand, WholesalePriceDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateWholesalePriceCommandHandler> _logger;

    public CreateWholesalePriceCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateWholesalePriceCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<WholesalePriceDto> Handle(CreateWholesalePriceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating wholesale price. ProductId: {ProductId}, OrganizationId: {OrganizationId}",
            request.Dto.ProductId, request.Dto.OrganizationId);

        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, handler'da tekrar validation gereksiz

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var product = await _context.Set<ProductEntity>()
            .FirstOrDefaultAsync(p => p.Id == request.Dto.ProductId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", request.Dto.ProductId);
        }

        OrganizationEntity? organization = null;
        if (request.Dto.OrganizationId.HasValue)
        {
            organization = await _context.Set<OrganizationEntity>()
                .FirstOrDefaultAsync(o => o.Id == request.Dto.OrganizationId.Value, cancellationToken);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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

        await _context.Set<WholesalePrice>().AddAsync(wholesalePrice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: Reload with Include for AutoMapper
        // ✅ PERFORMANCE: AsSplitQuery to avoid Cartesian Explosion (multiple Include'lar)
        wholesalePrice = await _context.Set<WholesalePrice>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting - Multiple Include'lar için
            .Include(wp => wp.Product)
            .Include(wp => wp.Organization)
            .FirstOrDefaultAsync(wp => wp.Id == wholesalePrice.Id, cancellationToken);

        _logger.LogInformation("Wholesale price created successfully. WholesalePriceId: {WholesalePriceId}", wholesalePrice!.Id);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<WholesalePriceDto>(wholesalePrice);
    }
}

