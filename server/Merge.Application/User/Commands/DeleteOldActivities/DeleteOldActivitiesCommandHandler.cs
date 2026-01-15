using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Commands.DeleteOldActivities;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteOldActivitiesCommandHandler : IRequestHandler<DeleteOldActivitiesCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteOldActivitiesCommandHandler> _logger;
    private readonly UserSettings _userSettings;

    public DeleteOldActivitiesCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteOldActivitiesCommandHandler> logger, IOptions<UserSettings> userSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _userSettings = userSettings.Value;
    }

    public async Task Handle(DeleteOldActivitiesCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)

        _logger.LogInformation("Deleting old activities older than {Days} days", request.DaysToKeep);
        var daysToKeep = request.DaysToKeep;
        if (daysToKeep < _userSettings.Activity.MinDaysToKeep) daysToKeep = _userSettings.Activity.MinDaysToKeep;
        if (daysToKeep > _userSettings.Activity.MaxDaysToKeep) daysToKeep = _userSettings.Activity.MaxDaysToKeep;

        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        var oldActivities = await _context.Set<UserActivityLog>()
            .Where(a => a.CreatedAt < cutoffDate)
            .ToListAsync(cancellationToken);

        _context.Set<UserActivityLog>().RemoveRange(oldActivities);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event\'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır

        _logger.LogWarning("Deleted {Count} old activity records", oldActivities.Count);
    }
}
