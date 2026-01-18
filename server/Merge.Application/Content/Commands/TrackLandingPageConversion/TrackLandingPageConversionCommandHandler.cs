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
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.LandingPage>;

namespace Merge.Application.Content.Commands.TrackLandingPageConversion;

public class TrackLandingPageConversionCommandHandler(
    IRepository landingPageRepository,
    IUnitOfWork unitOfWork,
    ILogger<TrackLandingPageConversionCommandHandler> logger) : IRequestHandler<TrackLandingPageConversionCommand, bool>
{

    public async Task<bool> Handle(TrackLandingPageConversionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Tracking conversion for landing page. LandingPageId: {LandingPageId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var landingPage = await landingPageRepository.GetByIdAsync(request.Id, cancellationToken);
            if (landingPage == null)
            {
                logger.LogWarning("Landing page not found for conversion tracking. LandingPageId: {LandingPageId}", request.Id);
                return false;
            }

            landingPage.TrackConversion();

            await landingPageRepository.UpdateAsync(landingPage, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Conversion tracked for landing page. LandingPageId: {LandingPageId}, ConversionCount: {ConversionCount}, ConversionRate: {ConversionRate}",
                request.Id, landingPage.ConversionCount, landingPage.ConversionRate);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while tracking conversion for landing page {LandingPageId}", request.Id);
            throw new BusinessException("Conversion takibi çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error tracking conversion for landing page {LandingPageId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

