using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.DeleteTemplate;

/// <summary>
/// Delete Template Command Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteTemplateCommandHandler> _logger;

    public DeleteTemplateCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteTemplateCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        template.Delete();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Notification template silindi. TemplateId: {TemplateId}",
            request.Id);

        return true;
    }
}
