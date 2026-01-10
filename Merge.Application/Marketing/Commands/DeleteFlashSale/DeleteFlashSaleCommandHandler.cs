using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Commands.DeleteFlashSale;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteFlashSaleCommandHandler : IRequestHandler<DeleteFlashSaleCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFlashSaleCommandHandler> _logger;

    public DeleteFlashSaleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteFlashSaleCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteFlashSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting flash sale. FlashSaleId: {FlashSaleId}", request.Id);

        var flashSale = await _context.Set<FlashSale>()
            .FirstOrDefaultAsync(fs => fs.Id == request.Id, cancellationToken);

        if (flashSale == null)
        {
            _logger.LogWarning("FlashSale not found. FlashSaleId: {FlashSaleId}", request.Id);
            throw new NotFoundException("Flash Sale", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı (soft delete)
        flashSale.MarkAsDeleted();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("FlashSale deleted successfully. FlashSaleId: {FlashSaleId}", request.Id);

        return true;
    }
}
