using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.CreateCommissionTier;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateCommissionTierCommandHandler : IRequestHandler<CreateCommissionTierCommand, CommissionTierDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCommissionTierCommandHandler> _logger;

    public CreateCommissionTierCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateCommissionTierCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CommissionTierDto> Handle(CreateCommissionTierCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Creating commission tier. Name: {Name}, CommissionRate: {CommissionRate}%",
            request.Name, request.CommissionRate);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var tier = CommissionTier.Create(
            name: request.Name,
            minSales: request.MinSales,
            maxSales: request.MaxSales,
            commissionRate: request.CommissionRate,
            platformFeeRate: request.PlatformFeeRate,
            priority: request.Priority);

        await _context.Set<CommissionTier>().AddAsync(tier, cancellationToken);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Commission tier created. TierId: {TierId}", tier.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<CommissionTierDto>(tier);
    }
}
