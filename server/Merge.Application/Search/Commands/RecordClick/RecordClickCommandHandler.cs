using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Search.Commands.RecordClick;

public class RecordClickCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<RecordClickCommandHandler> logger) : IRequestHandler<RecordClickCommand>
{

    public async Task Handle(RecordClickCommand request, CancellationToken cancellationToken)
    {
        var searchHistory = await context.Set<SearchHistory>()
            .FirstOrDefaultAsync(sh => sh.Id == request.SearchHistoryId, cancellationToken);

        if (searchHistory == null)
        {
            return;
        }

        searchHistory.RecordClick(request.ProductId);

        var popularSearch = await context.Set<PopularSearch>()
            .FirstOrDefaultAsync(ps => EF.Functions.ILike(ps.SearchTerm, searchHistory.SearchTerm), cancellationToken);

        if (popularSearch != null)
        {
            popularSearch.IncrementClickThroughCount();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Search click kaydedildi. SearchHistoryId: {SearchHistoryId}, ProductId: {ProductId}",
            request.SearchHistoryId, request.ProductId);
    }
}
