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

namespace Merge.Application.Support.Commands.DeleteKnowledgeBaseArticle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteKnowledgeBaseArticleCommandHandler : IRequestHandler<DeleteKnowledgeBaseArticleCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteKnowledgeBaseArticleCommandHandler> _logger;

    public DeleteKnowledgeBaseArticleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteKnowledgeBaseArticleCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteKnowledgeBaseArticleCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Deleting knowledge base article {ArticleId}", request.ArticleId);

        var article = await _context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == request.ArticleId, cancellationToken);

        if (article == null)
        {
            _logger.LogWarning("Knowledge base article {ArticleId} not found for deletion", request.ArticleId);
            throw new NotFoundException("Bilgi bankası makalesi", request.ArticleId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
        article.MarkAsDeleted();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Knowledge base article {ArticleId} deleted successfully", request.ArticleId);

        return true;
    }
}
