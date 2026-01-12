using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.DeleteB2BUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteB2BUserCommandHandler : IRequestHandler<DeleteB2BUserCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteB2BUserCommandHandler> _logger;

    public DeleteB2BUserCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteB2BUserCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteB2BUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting B2B user. B2BUserId: {B2BUserId}", request.Id);

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var b2bUser = await _context.Set<B2BUser>()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (b2bUser == null)
        {
            _logger.LogWarning("B2B user not found with Id: {B2BUserId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Soft delete (IsDeleted flag)
        // ✅ BOLUM 1.1: Rich Domain Model - Soft delete (IsDeleted flag)
        b2bUser.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("B2B user deleted successfully. B2BUserId: {B2BUserId}", request.Id);
        return true;
    }
}

