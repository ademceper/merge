using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Commands.DeletePageBuilder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeletePageBuilderCommandHandler : IRequestHandler<DeletePageBuilderCommand, bool>
{
    private readonly Merge.Application.Interfaces.IRepository<PageBuilder> _pageBuilderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<DeletePageBuilderCommandHandler> _logger;
    private const string CACHE_KEY_ALL_PAGES = "page_builders_all";
    private const string CACHE_KEY_ACTIVE_PAGES = "page_builders_active";

    public DeletePageBuilderCommandHandler(
        Merge.Application.Interfaces.IRepository<PageBuilder> pageBuilderRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<DeletePageBuilderCommandHandler> logger)
    {
        _pageBuilderRepository = pageBuilderRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeletePageBuilderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting page builder. PageBuilderId: {PageBuilderId}", request.Id);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var pageBuilder = await _pageBuilderRepository.GetByIdAsync(request.Id, cancellationToken);
            if (pageBuilder == null)
            {
                _logger.LogWarning("Page builder not found for deletion. PageBuilderId: {PageBuilderId}", request.Id);
                return false;
            }

            // ✅ BOLUM 3.2: IDOR Koruması - Manager sadece kendi page builder'larını silebilmeli (Admin hariç)
            if (request.PerformedBy.HasValue && pageBuilder.AuthorId.HasValue && pageBuilder.AuthorId.Value != request.PerformedBy.Value)
            {
                _logger.LogWarning("Unauthorized attempt to delete page builder {PageBuilderId} by user {UserId}. Page belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, pageBuilder.AuthorId.Value);
                throw new BusinessException("Bu page builder'ı silme yetkiniz bulunmamaktadır.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            pageBuilder.MarkAsDeleted();

            await _pageBuilderRepository.UpdateAsync(pageBuilder, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_PAGES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_PAGES, cancellationToken);
            await _cache.RemoveAsync($"page_builder_{request.Id}", cancellationToken);
            await _cache.RemoveAsync($"page_builder_slug_{pageBuilder.Slug}", cancellationToken);

            _logger.LogInformation("Page builder deleted (soft delete). PageBuilderId: {PageBuilderId}", request.Id);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while deleting page builder Id: {PageBuilderId}", request.Id);
            throw new BusinessException("Page builder silme çakışması. Başka bir kullanıcı aynı page builder'ı güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting page builder Id: {PageBuilderId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

