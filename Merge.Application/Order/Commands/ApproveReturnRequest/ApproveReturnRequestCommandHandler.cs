using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Commands.ApproveReturnRequest;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ApproveReturnRequestCommandHandler : IRequestHandler<ApproveReturnRequestCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApproveReturnRequestCommandHandler> _logger;

    public ApproveReturnRequestCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ApproveReturnRequestCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ApproveReturnRequestCommand request, CancellationToken cancellationToken)
    {
        var returnRequest = await _context.Set<ReturnRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.ReturnRequestId, cancellationToken);

        if (returnRequest == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        returnRequest.Approve();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Return request approved. ReturnRequestId: {ReturnRequestId}", request.ReturnRequestId);

        return true;
    }
}
