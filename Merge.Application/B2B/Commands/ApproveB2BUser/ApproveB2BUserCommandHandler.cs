using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.B2B.Commands.ApproveB2BUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ApproveB2BUserCommandHandler : IRequestHandler<ApproveB2BUserCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApproveB2BUserCommandHandler> _logger;

    public ApproveB2BUserCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ApproveB2BUserCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ApproveB2BUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving B2B user. B2BUserId: {B2BUserId}, ApprovedByUserId: {ApprovedByUserId}",
            request.Id, request.ApprovedByUserId);

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var b2bUser = await _context.Set<B2BUser>()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (b2bUser == null)
        {
            _logger.LogWarning("B2B user not found with Id: {B2BUserId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        // ✅ ARCHITECTURE: Domain event'ler entity içinde oluşturuluyor (B2BUser.Approve() içinde B2BUserApprovedEvent)
        b2bUser.Approve(request.ApprovedByUserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("B2B user approved successfully. B2BUserId: {B2BUserId}", request.Id);
        return true;
    }
}

