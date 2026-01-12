using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.DeleteEmailAutomation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteEmailAutomationCommandHandler : IRequestHandler<DeleteEmailAutomationCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteEmailAutomationCommandHandler> _logger;

    public DeleteEmailAutomationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteEmailAutomationCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteEmailAutomationCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var automation = await _context.Set<Merge.Domain.Modules.Notifications.EmailAutomation>()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (automation == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        // ✅ BOLUM 1.3: Soft Delete (ZORUNLU)
        automation.MarkAsDeleted();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email otomasyonu silindi. AutomationId: {AutomationId}",
            request.Id);

        return true;
    }
}
