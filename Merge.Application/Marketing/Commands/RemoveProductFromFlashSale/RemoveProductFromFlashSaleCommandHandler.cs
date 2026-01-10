using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Commands.RemoveProductFromFlashSale;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RemoveProductFromFlashSaleCommandHandler : IRequestHandler<RemoveProductFromFlashSaleCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveProductFromFlashSaleCommandHandler> _logger;

    public RemoveProductFromFlashSaleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RemoveProductFromFlashSaleCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RemoveProductFromFlashSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing product from flash sale. FlashSaleId: {FlashSaleId}, ProductId: {ProductId}", 
            request.FlashSaleId, request.ProductId);

        var flashSaleProduct = await _context.Set<FlashSaleProduct>()
            .FirstOrDefaultAsync(fsp => fsp.FlashSaleId == request.FlashSaleId && fsp.ProductId == request.ProductId, cancellationToken);

        if (flashSaleProduct == null)
        {
            _logger.LogWarning("FlashSaleProduct not found. FlashSaleId: {FlashSaleId}, ProductId: {ProductId}", 
                request.FlashSaleId, request.ProductId);
            throw new NotFoundException("Flash Sale Product", Guid.Empty);
        }

        _context.Set<FlashSaleProduct>().Remove(flashSaleProduct);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product removed from flash sale successfully. FlashSaleId: {FlashSaleId}, ProductId: {ProductId}", 
            request.FlashSaleId, request.ProductId);

        return true;
    }
}
