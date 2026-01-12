using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.TrackEmailOpen;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class TrackEmailOpenCommandHandler : IRequestHandler<TrackEmailOpenCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TrackEmailOpenCommandHandler> _logger;

    public TrackEmailOpenCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<TrackEmailOpenCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(TrackEmailOpenCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var email = await _context.Set<AbandonedCartEmail>()
            .FirstOrDefaultAsync(e => e.Id == request.EmailId, cancellationToken);

        if (email == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        email.MarkAsOpened();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}

