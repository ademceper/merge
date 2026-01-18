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

public class PublishKnowledgeBaseArticleCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<PublishKnowledgeBaseArticleCommandHandler> logger) : IRequestHandler<PublishKnowledgeBaseArticleCommand, bool>
{

    public async Task<bool> Handle(PublishKnowledgeBaseArticleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Publishing knowledge base article {ArticleId}", request.ArticleId);

        var article = await context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == request.ArticleId, cancellationToken);

        if (article == null)
        {
            logger.LogWarning("Knowledge base article {ArticleId} not found for publishing", request.ArticleId);
            throw new NotFoundException("Bilgi bankası makalesi", request.ArticleId);
        }

        article.Publish();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Knowledge base article {ArticleId} published successfully", request.ArticleId);

        return true;
    }
}
