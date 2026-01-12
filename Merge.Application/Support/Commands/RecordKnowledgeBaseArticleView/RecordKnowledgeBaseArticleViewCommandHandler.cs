using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Commands.RecordKnowledgeBaseArticleView;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RecordKnowledgeBaseArticleViewCommandHandler : IRequestHandler<RecordKnowledgeBaseArticleViewCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecordKnowledgeBaseArticleViewCommandHandler> _logger;

    public RecordKnowledgeBaseArticleViewCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RecordKnowledgeBaseArticleViewCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RecordKnowledgeBaseArticleViewCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Recording view for knowledge base article {ArticleId}. UserId: {UserId}",
            request.ArticleId, request.UserId);

        var article = await _context.Set<KnowledgeBaseArticle>()
            .FirstOrDefaultAsync(a => a.Id == request.ArticleId, cancellationToken);

        if (article == null)
        {
            _logger.LogWarning("Knowledge base article {ArticleId} not found for view recording", request.ArticleId);
            throw new NotFoundException("Bilgi bankası makalesi", request.ArticleId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        var view = article.RecordView(request.UserId, request.IpAddress, request.UserAgent);
        
        // ✅ BOLUM 1.1: Rich Domain Model - View entity'sini ekle
        await _context.Set<KnowledgeBaseView>().AddAsync(view, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("View recorded for knowledge base article {ArticleId}. New view count: {ViewCount}",
            request.ArticleId, article.ViewCount);

        return true;
    }
}
