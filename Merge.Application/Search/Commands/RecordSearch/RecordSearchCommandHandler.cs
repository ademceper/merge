using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Search.Commands.RecordSearch;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RecordSearchCommandHandler : IRequestHandler<RecordSearchCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecordSearchCommandHandler> _logger;

    public RecordSearchCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RecordSearchCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(RecordSearchCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return;
        }

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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

        await _context.Set<SearchHistory>().AddAsync(searchHistory, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !ps.IsDeleted (Global Query Filter)
        var popularSearch = await _context.Set<PopularSearch>()
            .FirstOrDefaultAsync(ps => ps.SearchTerm.ToLower() == normalizedTerm.ToLower(), cancellationToken);

        if (popularSearch == null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            popularSearch = PopularSearch.Create(normalizedTerm);
            await _context.Set<PopularSearch>().AddAsync(popularSearch, cancellationToken);
        }
        else
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            popularSearch.IncrementSearchCount();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Search kaydedildi. SearchTerm: {SearchTerm}, UserId: {UserId}",
            request.SearchTerm, request.UserId);
    }
}
