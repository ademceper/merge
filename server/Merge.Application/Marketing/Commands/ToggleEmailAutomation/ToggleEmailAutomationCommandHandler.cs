using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using EmailAutomation = Merge.Domain.Modules.Notifications.EmailAutomation;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.ToggleEmailAutomation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class ToggleEmailAutomationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ToggleEmailAutomationCommandHandler> logger) : IRequestHandler<ToggleEmailAutomationCommand, bool>
{
    public async Task<bool> Handle(ToggleEmailAutomationCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var automation = await context.Set<EmailAutomation>()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (automation == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (request.IsActive)
            automation.Activate();
        else
            automation.Deactivate();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Email otomasyonu durumu değiştirildi. AutomationId: {AutomationId}, IsActive: {IsActive}",
            request.Id, request.IsActive);

        return true;
    }
}
