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
public class TrackEmailOpenCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<TrackEmailOpenCommandHandler> logger) : IRequestHandler<TrackEmailOpenCommand, bool>
{

    public async Task<bool> Handle(TrackEmailOpenCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var email = await context.Set<AbandonedCartEmail>()
            .FirstOrDefaultAsync(e => e.Id == request.EmailId, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (email is null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        email.MarkAsOpened();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}

