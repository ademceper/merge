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
public class UpdateFlashSaleCommandHandler : IRequestHandler<UpdateFlashSaleCommand, FlashSaleDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateFlashSaleCommandHandler> _logger;

    public UpdateFlashSaleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateFlashSaleCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FlashSaleDto> Handle(UpdateFlashSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating flash sale. FlashSaleId: {FlashSaleId}", request.Id);

        var flashSale = await _context.Set<FlashSale>()
            .FirstOrDefaultAsync(fs => fs.Id == request.Id, cancellationToken);

        if (flashSale == null)
        {
            _logger.LogWarning("FlashSale not found. FlashSaleId: {FlashSaleId}", request.Id);
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + AsSplitQuery ile tek query'de getir (N+1 query önleme)
        var updatedFlashSale = await _context.Set<FlashSale>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(fs => fs.FlashSaleProducts)
                .ThenInclude(fsp => fsp.Product)
            .FirstOrDefaultAsync(fs => fs.Id == flashSale.Id, cancellationToken);

        if (updatedFlashSale == null)
        {
            _logger.LogWarning("FlashSale not found after update. FlashSaleId: {FlashSaleId}", flashSale.Id);
            throw new NotFoundException("Flash Sale", flashSale.Id);
        }

        _logger.LogInformation("FlashSale updated successfully. FlashSaleId: {FlashSaleId}", request.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FlashSaleDto>(updatedFlashSale);
    }
}
