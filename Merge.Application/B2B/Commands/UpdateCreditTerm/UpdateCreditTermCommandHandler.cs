using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.UpdateCreditTerm;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateCreditTermCommandHandler : IRequestHandler<UpdateCreditTermCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCreditTermCommandHandler> _logger;

    public UpdateCreditTermCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateCreditTermCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateCreditTermCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating credit term. CreditTermId: {CreditTermId}", request.Id);

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var creditTerm = await _context.Set<CreditTerm>()
            .FirstOrDefaultAsync(ct => ct.Id == request.Id, cancellationToken);

        if (creditTerm == null)
        {
            _logger.LogWarning("Credit term not found with Id: {CreditTermId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        creditTerm.UpdateDetails(request.Dto.Name, request.Dto.PaymentDays, request.Dto.Terms);
        creditTerm.UpdateCreditLimit(request.Dto.CreditLimit);
        creditTerm.UpdatedAt = DateTime.UtcNow;
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Credit term updated successfully. CreditTermId: {CreditTermId}", request.Id);
        return true;
    }
}

