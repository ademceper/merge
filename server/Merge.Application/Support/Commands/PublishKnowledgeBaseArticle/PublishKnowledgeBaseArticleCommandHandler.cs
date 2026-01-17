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

namespace Merge.Application.Support.Commands.PublishKnowledgeBaseArticle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class PublishKnowledgeBaseArticleCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<PublishKnowledgeBaseArticleCommandHandler> logger) : IRequestHandler<PublishKnowledgeBaseArticleCommand, bool>
{

    public async Task<bool> Handle(PublishKnowledgeBaseArticleCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Publishing knowledge base article {ArticleId}", request.ArticleId);

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var article = await context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == request.ArticleId, cancellationToken);

        if (article == null)
        {
            logger.LogWarning("Knowledge base article {ArticleId} not found for publishing", request.ArticleId);
            throw new NotFoundException("Bilgi bankası makalesi", request.ArticleId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        article.Publish();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Knowledge base article {ArticleId} published successfully", request.ArticleId);

        return true;
    }
}
