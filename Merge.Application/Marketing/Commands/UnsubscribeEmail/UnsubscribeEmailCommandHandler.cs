using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.UnsubscribeEmail;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UnsubscribeEmailCommandHandler : IRequestHandler<UnsubscribeEmailCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnsubscribeEmailCommandHandler> _logger;

    public UnsubscribeEmailCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UnsubscribeEmailCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(UnsubscribeEmailCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var subscriber = await _context.Set<Merge.Domain.Modules.Marketing.EmailSubscriber>()
            .FirstOrDefaultAsync(s => s.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (subscriber == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        subscriber.Unsubscribe();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email aboneliği iptal edildi. Email: {Email}",
            request.Email);

        return true;
    }
}
