using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.DeleteEmailCampaign;

public class DeleteEmailCampaignCommandHandler : IRequestHandler<DeleteEmailCampaignCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteEmailCampaignCommandHandler> _logger;

    public DeleteEmailCampaignCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteEmailCampaignCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteEmailCampaignCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Deleting email campaign. CampaignId: {CampaignId}", request.Id);

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (campaign == null)
        {
            _logger.LogWarning("Email campaign not found. CampaignId: {CampaignId}", request.Id);
            return false;
        }

        if (campaign.Status == Merge.Domain.Enums.EmailCampaignStatus.Sending)
        {
            _logger.LogWarning("Cannot delete campaign that is currently sending. CampaignId: {CampaignId}", request.Id);
            throw new BusinessException("Şu anda gönderilmekte olan bir kampanya silinemez.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        campaign.MarkAsDeleted();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Email campaign deleted successfully. CampaignId: {CampaignId}", request.Id);

        return true;
    }
}
