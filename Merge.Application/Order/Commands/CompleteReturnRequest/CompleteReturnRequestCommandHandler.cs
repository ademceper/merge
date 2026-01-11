using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Order.Commands.CompleteReturnRequest;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CompleteReturnRequestCommandHandler : IRequestHandler<CompleteReturnRequestCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompleteReturnRequestCommandHandler> _logger;

    public CompleteReturnRequestCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<CompleteReturnRequestCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(CompleteReturnRequestCommand request, CancellationToken cancellationToken)
    {
        var returnRequest = await _context.Set<ReturnRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.ReturnRequestId, cancellationToken);

        if (returnRequest == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        returnRequest.Complete(request.TrackingNumber);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Return request completed. ReturnRequestId: {ReturnRequestId}, TrackingNumber: {TrackingNumber}", 
            request.ReturnRequestId, request.TrackingNumber);

        return true;
    }
}
