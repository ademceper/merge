using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.GenerateShareCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GenerateShareCodeCommandHandler : IRequestHandler<GenerateShareCodeCommand, string>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GenerateShareCodeCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_COMPARISON_BY_SHARE_CODE = "comparison_by_share_code_";
    private const string CACHE_KEY_COMPARISON_BY_ID = "comparison_by_id_";

    public GenerateShareCodeCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<GenerateShareCodeCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<string> Handle(GenerateShareCodeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating share code. ComparisonId: {ComparisonId}", request.ComparisonId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var comparison = await _context.Set<ProductComparison>()
                .FirstOrDefaultAsync(c => c.Id == request.ComparisonId, cancellationToken);

            if (comparison == null)
            {
                throw new NotFoundException("Karşılaştırma", request.ComparisonId);
            }

            if (!string.IsNullOrEmpty(comparison.ShareCode))
            {
                return comparison.ShareCode;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            // Domain method içinde zaten unique share code generation var
            comparison.GenerateShareCode();

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            if (!string.IsNullOrEmpty(comparison.ShareCode))
            {
                await _cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_SHARE_CODE}{comparison.ShareCode}", cancellationToken);
            }
            await _cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_ID}{request.ComparisonId}", cancellationToken);

            _logger.LogInformation("Share code generated successfully. ComparisonId: {ComparisonId}, ShareCode: {ShareCode}",
                request.ComparisonId, comparison.ShareCode);

            return comparison.ShareCode!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating share code. ComparisonId: {ComparisonId}", request.ComparisonId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

}
