using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Commands.PublishKnowledgeBaseArticle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class PublishKnowledgeBaseArticleCommandHandler : IRequestHandler<PublishKnowledgeBaseArticleCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PublishKnowledgeBaseArticleCommandHandler> _logger;

    public PublishKnowledgeBaseArticleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<PublishKnowledgeBaseArticleCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(PublishKnowledgeBaseArticleCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Publishing knowledge base article {ArticleId}", request.ArticleId);

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var article = await _context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == request.ArticleId, cancellationToken);

        if (article == null)
        {
            _logger.LogWarning("Knowledge base article {ArticleId} not found for publishing", request.ArticleId);
            throw new NotFoundException("Bilgi bankası makalesi", request.ArticleId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        article.Publish();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Knowledge base article {ArticleId} published successfully", request.ArticleId);

        return true;
    }
}
