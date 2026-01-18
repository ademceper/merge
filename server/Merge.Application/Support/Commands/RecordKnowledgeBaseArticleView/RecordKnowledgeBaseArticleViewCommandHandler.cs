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

namespace Merge.Application.Support.Commands.RecordKnowledgeBaseArticleView;

public class RecordKnowledgeBaseArticleViewCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<RecordKnowledgeBaseArticleViewCommandHandler> logger) : IRequestHandler<RecordKnowledgeBaseArticleViewCommand, bool>
{

    public async Task<bool> Handle(RecordKnowledgeBaseArticleViewCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Recording view for knowledge base article {ArticleId}. UserId: {UserId}",
            request.ArticleId, request.UserId);

        var article = await context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == request.ArticleId, cancellationToken);

        if (article == null)
        {
            logger.LogWarning("Knowledge base article {ArticleId} not found for view recording", request.ArticleId);
            throw new NotFoundException("Bilgi bankası makalesi", request.ArticleId);
        }

        var view = article.RecordView(request.UserId, request.IpAddress, request.UserAgent);
        
        await context.Set<KnowledgeBaseView>().AddAsync(view, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("View recorded for knowledge base article {ArticleId}. New view count: {ViewCount}",
            request.ArticleId, article.ViewCount);

        return true;
    }
}
