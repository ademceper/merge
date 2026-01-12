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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RecordClickCommandHandler : IRequestHandler<RecordClickCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecordClickCommandHandler> _logger;

    public RecordClickCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RecordClickCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(RecordClickCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !sh.IsDeleted (Global Query Filter)
        var searchHistory = await _context.Set<SearchHistory>()
            .FirstOrDefaultAsync(sh => sh.Id == request.SearchHistoryId, cancellationToken);

        if (searchHistory == null)
        {
            return;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        searchHistory.RecordClick(request.ProductId);

        // ✅ PERFORMANCE: Removed manual !ps.IsDeleted (Global Query Filter)
        var popularSearch = await _context.Set<PopularSearch>()
            .FirstOrDefaultAsync(ps => ps.SearchTerm.ToLower() == searchHistory.SearchTerm.ToLower(), cancellationToken);

        if (popularSearch != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            popularSearch.IncrementClickThroughCount();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Search click kaydedildi. SearchHistoryId: {SearchHistoryId}, ProductId: {ProductId}",
            request.SearchHistoryId, request.ProductId);
    }
}
