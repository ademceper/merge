using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Search.Commands.RecordSearch;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RecordSearchCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<RecordSearchCommandHandler> logger) : IRequestHandler<RecordSearchCommand>
{

    public async Task Handle(RecordSearchCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return;
        }

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Search kaydediliyor. SearchTerm: {SearchTerm}, UserId: {UserId}, ResultCount: {ResultCount}",
            request.SearchTerm, request.UserId, request.ResultCount);

        var normalizedTerm = request.SearchTerm.Trim();

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var searchHistory = SearchHistory.Create(
            request.UserId,
            normalizedTerm,
            request.ResultCount,
            request.UserAgent,
            request.IpAddress);

        await context.Set<SearchHistory>().AddAsync(searchHistory, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !ps.IsDeleted (Global Query Filter)
        var popularSearch = await context.Set<PopularSearch>()
            .FirstOrDefaultAsync(ps => ps.SearchTerm.ToLower() == normalizedTerm.ToLower(), cancellationToken);

        if (popularSearch == null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            popularSearch = PopularSearch.Create(normalizedTerm);
            await context.Set<PopularSearch>().AddAsync(popularSearch, cancellationToken);
        }
        else
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            popularSearch.IncrementSearchCount();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Search kaydedildi. SearchTerm: {SearchTerm}, UserId: {UserId}",
            request.SearchTerm, request.UserId);
    }
}
