using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Order.Commands.RejectReturnRequest;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RejectReturnRequestCommandHandler : IRequestHandler<RejectReturnRequestCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RejectReturnRequestCommandHandler> _logger;

    public RejectReturnRequestCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RejectReturnRequestCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RejectReturnRequestCommand request, CancellationToken cancellationToken)
    {
        var returnRequest = await _context.Set<ReturnRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.ReturnRequestId, cancellationToken);

        if (returnRequest == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        returnRequest.Reject(request.Reason);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Return request rejected. ReturnRequestId: {ReturnRequestId}, Reason: {Reason}", 
            request.ReturnRequestId, request.Reason);

        return true;
    }
}
