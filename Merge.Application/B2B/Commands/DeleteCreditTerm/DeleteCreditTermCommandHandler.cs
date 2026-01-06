using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.B2B.Commands.DeleteCreditTerm;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteCreditTermCommandHandler : IRequestHandler<DeleteCreditTermCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCreditTermCommandHandler> _logger;

    public DeleteCreditTermCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteCreditTermCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteCreditTermCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting credit term. CreditTermId: {CreditTermId}", request.Id);

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var creditTerm = await _context.Set<CreditTerm>()
            .FirstOrDefaultAsync(ct => ct.Id == request.Id, cancellationToken);

        if (creditTerm == null)
        {
            _logger.LogWarning("Credit term not found with Id: {CreditTermId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Soft delete (IsDeleted flag)
        creditTerm.IsDeleted = true;
        creditTerm.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Credit term deleted successfully. CreditTermId: {CreditTermId}", request.Id);
        return true;
    }
}

