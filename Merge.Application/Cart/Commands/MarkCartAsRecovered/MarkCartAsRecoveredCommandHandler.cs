using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Cart.Commands.MarkCartAsRecovered;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class MarkCartAsRecoveredCommandHandler : IRequestHandler<MarkCartAsRecoveredCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkCartAsRecoveredCommandHandler> _logger;

    public MarkCartAsRecoveredCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<MarkCartAsRecoveredCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(MarkCartAsRecoveredCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var emails = await _context.Set<AbandonedCartEmail>()
            .Where(e => e.CartId == request.CartId)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        foreach (var email in emails)
        {
            email.MarkAsResultedInPurchase();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

