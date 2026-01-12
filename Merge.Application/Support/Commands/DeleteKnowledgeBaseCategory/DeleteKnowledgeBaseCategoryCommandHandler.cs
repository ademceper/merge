using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.DeleteKnowledgeBaseCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteKnowledgeBaseCategoryCommandHandler : IRequestHandler<DeleteKnowledgeBaseCategoryCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteKnowledgeBaseCategoryCommandHandler> _logger;

    public DeleteKnowledgeBaseCategoryCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteKnowledgeBaseCategoryCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteKnowledgeBaseCategoryCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Deleting knowledge base category {CategoryId}", request.CategoryId);

        var category = await _context.Set<KnowledgeBaseCategory>()
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Knowledge base category {CategoryId} not found for deletion", request.CategoryId);
            throw new NotFoundException("Bilgi bankası kategorisi", request.CategoryId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
        category.MarkAsDeleted();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Knowledge base category {CategoryId} deleted successfully", request.CategoryId);

        return true;
    }
}
