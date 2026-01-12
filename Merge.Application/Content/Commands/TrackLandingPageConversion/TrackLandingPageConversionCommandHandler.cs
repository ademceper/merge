using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Commands.TrackLandingPageConversion;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class TrackLandingPageConversionCommandHandler : IRequestHandler<TrackLandingPageConversionCommand, bool>
{
    private readonly Merge.Application.Interfaces.IRepository<LandingPage> _landingPageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TrackLandingPageConversionCommandHandler> _logger;

    public TrackLandingPageConversionCommandHandler(
        Merge.Application.Interfaces.IRepository<LandingPage> landingPageRepository,
        IUnitOfWork unitOfWork,
        ILogger<TrackLandingPageConversionCommandHandler> logger)
    {
        _landingPageRepository = landingPageRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(TrackLandingPageConversionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Tracking conversion for landing page. LandingPageId: {LandingPageId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var landingPage = await _landingPageRepository.GetByIdAsync(request.Id, cancellationToken);
            if (landingPage == null)
            {
                _logger.LogWarning("Landing page not found for conversion tracking. LandingPageId: {LandingPageId}", request.Id);
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            landingPage.TrackConversion();

            await _landingPageRepository.UpdateAsync(landingPage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Conversion tracked for landing page. LandingPageId: {LandingPageId}, ConversionCount: {ConversionCount}, ConversionRate: {ConversionRate}",
                request.Id, landingPage.ConversionCount, landingPage.ConversionRate);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while tracking conversion for landing page {LandingPageId}", request.Id);
            throw new BusinessException("Conversion takibi çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error tracking conversion for landing page {LandingPageId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

