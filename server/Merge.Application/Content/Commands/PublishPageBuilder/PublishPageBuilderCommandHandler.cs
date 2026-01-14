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

namespace Merge.Application.Content.Commands.PublishPageBuilder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class PublishPageBuilderCommandHandler : IRequestHandler<PublishPageBuilderCommand, bool>
{
    private readonly Merge.Application.Interfaces.IRepository<PageBuilder> _pageBuilderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<PublishPageBuilderCommandHandler> _logger;
    private const string CACHE_KEY_ALL_PAGES = "page_builders_all";
    private const string CACHE_KEY_ACTIVE_PAGES = "page_builders_active";

    public PublishPageBuilderCommandHandler(
        Merge.Application.Interfaces.IRepository<PageBuilder> pageBuilderRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<PublishPageBuilderCommandHandler> logger)
    {
        _pageBuilderRepository = pageBuilderRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(PublishPageBuilderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing page builder. PageBuilderId: {PageBuilderId}", request.Id);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var pageBuilder = await _pageBuilderRepository.GetByIdAsync(request.Id, cancellationToken);
            if (pageBuilder == null)
            {
                _logger.LogWarning("Page builder not found for publishing. PageBuilderId: {PageBuilderId}", request.Id);
                return false;
            }

            // ✅ BOLUM 3.2: IDOR Koruması - Manager sadece kendi page builder'larını yayınlayabilmeli (Admin hariç)
            if (request.PerformedBy.HasValue && pageBuilder.AuthorId.HasValue && pageBuilder.AuthorId.Value != request.PerformedBy.Value)
            {
                _logger.LogWarning("Unauthorized attempt to publish page builder {PageBuilderId} by user {UserId}. Page belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, pageBuilder.AuthorId.Value);
                throw new BusinessException("Bu page builder'ı yayınlama yetkiniz bulunmamaktadır.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            pageBuilder.Publish();

            await _pageBuilderRepository.UpdateAsync(pageBuilder, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_PAGES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_PAGES, cancellationToken);
            await _cache.RemoveAsync($"page_builder_{pageBuilder.Id}", cancellationToken);
            await _cache.RemoveAsync($"page_builder_slug_{pageBuilder.Slug}", cancellationToken);

            _logger.LogInformation("Page builder published. PageBuilderId: {PageBuilderId}", request.Id);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while publishing page builder {PageBuilderId}", request.Id);
            throw new BusinessException("Page builder yayınlama çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing page builder {PageBuilderId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

