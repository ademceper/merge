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
public class GenerateShareCodeCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<GenerateShareCodeCommandHandler> logger, ICacheService cache) : IRequestHandler<GenerateShareCodeCommand, string>
{

    private const string CACHE_KEY_COMPARISON_BY_SHARE_CODE = "comparison_by_share_code_";
    private const string CACHE_KEY_COMPARISON_BY_ID = "comparison_by_id_";

    public async Task<string> Handle(GenerateShareCodeCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating share code. ComparisonId: {ComparisonId}", request.ComparisonId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var comparison = await context.Set<ProductComparison>()
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
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            if (!string.IsNullOrEmpty(comparison.ShareCode))
            {
                await cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_SHARE_CODE}{comparison.ShareCode}", cancellationToken);
            }
            await cache.RemoveAsync($"{CACHE_KEY_COMPARISON_BY_ID}{request.ComparisonId}", cancellationToken);

            logger.LogInformation("Share code generated successfully. ComparisonId: {ComparisonId}, ShareCode: {ShareCode}",
                request.ComparisonId, comparison.ShareCode);

            return comparison.ShareCode!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating share code. ComparisonId: {ComparisonId}", request.ComparisonId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

}
