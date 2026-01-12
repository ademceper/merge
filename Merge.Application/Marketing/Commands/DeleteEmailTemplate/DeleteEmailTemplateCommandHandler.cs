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

namespace Merge.Application.Marketing.Commands.DeleteEmailTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteEmailTemplateCommandHandler : IRequestHandler<DeleteEmailTemplateCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteEmailTemplateCommandHandler> _logger;

    public DeleteEmailTemplateCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteEmailTemplateCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        // ✅ BOLUM 1.3: Soft Delete (ZORUNLU)
        template.MarkAsDeleted();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email template silindi. TemplateId: {TemplateId}",
            request.Id);

        return true;
    }
}
