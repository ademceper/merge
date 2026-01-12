using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.UpdateCreditUsage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateCreditUsageCommandHandler : IRequestHandler<UpdateCreditUsageCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCreditUsageCommandHandler> _logger;

    public UpdateCreditUsageCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateCreditUsageCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateCreditUsageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating credit usage. CreditTermId: {CreditTermId}, Amount: {Amount}",
            request.CreditTermId, request.Amount);

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var creditTerm = await _context.Set<CreditTerm>()
            .FirstOrDefaultAsync(ct => ct.Id == request.CreditTermId, cancellationToken);

        if (creditTerm == null)
        {
            _logger.LogWarning("Credit term not found with Id: {CreditTermId}", request.CreditTermId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        creditTerm.UseCredit(request.Amount);
        // creditTerm.UpdatedAt = DateTime.UtcNow; // Handled by entity
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Credit usage updated successfully. CreditTermId: {CreditTermId}", request.CreditTermId);
        return true;
    }
}

