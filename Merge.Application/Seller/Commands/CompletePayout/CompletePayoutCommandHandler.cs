using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Services.Notification;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Commands.CompletePayout;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CompletePayoutCommandHandler : IRequestHandler<CompletePayoutCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<CompletePayoutCommandHandler> _logger;

    public CompletePayoutCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<CompletePayoutCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(CompletePayoutCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Completing payout. PayoutId: {PayoutId}", request.PayoutId);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var payout = await _context.Set<CommissionPayout>()
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == request.PayoutId, cancellationToken);

        if (payout == null)
        {
            _logger.LogWarning("Payout not found. PayoutId: {PayoutId}", request.PayoutId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        payout.Complete();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send confirmation email
        await _emailService.SendEmailAsync(
            payout.Seller?.Email ?? string.Empty,
            $"Payout Completed - {payout.PayoutNumber}",
            $"Your payout of {payout.NetAmount:C} has been completed. Transaction Reference: {payout.TransactionReference}",
            true,
            cancellationToken
        );

        _logger.LogInformation("Payout completed. PayoutId: {PayoutId}, PayoutNumber: {PayoutNumber}",
            request.PayoutId, payout.PayoutNumber);

        return true;
    }
}
