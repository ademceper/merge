using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Support.Commands.UpdateCustomerCommunicationStatus;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateCustomerCommunicationStatusCommandHandler : IRequestHandler<UpdateCustomerCommunicationStatusCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCustomerCommunicationStatusCommandHandler> _logger;

    public UpdateCustomerCommunicationStatusCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateCustomerCommunicationStatusCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateCustomerCommunicationStatusCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Updating customer communication status. CommunicationId: {CommunicationId}, NewStatus: {Status}",
            request.CommunicationId, request.Status);

        var communication = await _context.Set<CustomerCommunication>()
            .FirstOrDefaultAsync(c => c.Id == request.CommunicationId, cancellationToken);

        if (communication == null)
        {
            _logger.LogWarning("Customer communication {CommunicationId} not found for status update", request.CommunicationId);
            throw new NotFoundException("Müşteri iletişimi", request.CommunicationId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        var newStatus = Enum.Parse<CommunicationStatus>(request.Status, true);
        communication.UpdateStatus(newStatus, request.DeliveredAt, request.ReadAt);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer communication {CommunicationId} status updated to {Status} successfully",
            request.CommunicationId, request.Status);

        return true;
    }
}
