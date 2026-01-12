using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.DeleteFaq;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteFaqCommandHandler : IRequestHandler<DeleteFaqCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteFaqCommandHandler> _logger;

    public DeleteFaqCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteFaqCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteFaqCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Deleting FAQ {FaqId}", request.FaqId);

        var faq = await _context.Set<FAQ>()
            .FirstOrDefaultAsync(f => f.Id == request.FaqId, cancellationToken);

        if (faq == null)
        {
            _logger.LogWarning("FAQ {FaqId} not found for deletion", request.FaqId);
            throw new NotFoundException("FAQ", request.FaqId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
        faq.MarkAsDeleted();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("FAQ {FaqId} deleted successfully", request.FaqId);

        return true;
    }
}
