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

public class RecordSearchCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<RecordSearchCommandHandler> logger) : IRequestHandler<RecordSearchCommand>
{

    public async Task Handle(RecordSearchCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return;
        }

        logger.LogInformation(
            "Search kaydediliyor. SearchTerm: {SearchTerm}, UserId: {UserId}, ResultCount: {ResultCount}",
            request.SearchTerm, request.UserId, request.ResultCount);

        var normalizedTerm = request.SearchTerm.Trim();

        var searchHistory = SearchHistory.Create(
            request.UserId,
            normalizedTerm,
            request.ResultCount,
            request.UserAgent,
            request.IpAddress);

        await context.Set<SearchHistory>().AddAsync(searchHistory, cancellationToken);

        var popularSearch = await context.Set<PopularSearch>()
            .FirstOrDefaultAsync(ps => EF.Functions.ILike(ps.SearchTerm, normalizedTerm), cancellationToken);

        if (popularSearch is null)
        {
            popularSearch = PopularSearch.Create(normalizedTerm);
            await context.Set<PopularSearch>().AddAsync(popularSearch, cancellationToken);
        }
        else
        {
            popularSearch.IncrementSearchCount();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Search kaydedildi. SearchTerm: {SearchTerm}, UserId: {UserId}",
            request.SearchTerm, request.UserId);
    }
}
