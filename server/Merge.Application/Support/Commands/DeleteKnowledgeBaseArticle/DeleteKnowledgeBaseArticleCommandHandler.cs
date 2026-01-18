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

public class DeleteKnowledgeBaseArticleCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteKnowledgeBaseArticleCommandHandler> logger) : IRequestHandler<DeleteKnowledgeBaseArticleCommand, bool>
{

    public async Task<bool> Handle(DeleteKnowledgeBaseArticleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting knowledge base article {ArticleId}", request.ArticleId);

        var article = await context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == request.ArticleId, cancellationToken);

        if (article == null)
        {
            logger.LogWarning("Knowledge base article {ArticleId} not found for deletion", request.ArticleId);
            throw new NotFoundException("Bilgi bankası makalesi", request.ArticleId);
        }

        article.MarkAsDeleted();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Knowledge base article {ArticleId} deleted successfully", request.ArticleId);

        return true;
    }
}
