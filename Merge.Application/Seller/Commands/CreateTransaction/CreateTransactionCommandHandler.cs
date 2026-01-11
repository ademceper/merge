using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Commands.CreateTransaction;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, SellerTransactionDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateTransactionCommandHandler> _logger;

    public CreateTransactionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateTransactionCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SellerTransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Creating transaction. SellerId: {SellerId}, Type: {TransactionType}, Amount: {Amount}",
            request.SellerId, request.TransactionType, request.Amount);

        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var seller = await _context.Set<SellerProfile>()
            .FirstOrDefaultAsync(sp => sp.UserId == request.SellerId, cancellationToken);

        if (seller == null)
        {
            _logger.LogWarning("Seller not found. SellerId: {SellerId}", request.SellerId);
            throw new NotFoundException("Satıcı", request.SellerId);
        }

        var balanceBefore = seller.AvailableBalance;

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var transaction = SellerTransaction.Create(
            sellerId: request.SellerId,
            transactionType: request.TransactionType,
            description: request.Description,
            amount: request.Amount,
            balanceBefore: balanceBefore,
            relatedEntityId: request.RelatedEntityId,
            relatedEntityType: request.RelatedEntityType);

        // Update seller balance using domain methods
        if (request.Amount > 0)
        {
            seller.AddEarnings(request.Amount);
        }
        else
        {
            seller.DeductFromAvailableBalance(Math.Abs(request.Amount));
        }

        await _context.Set<SellerTransaction>().AddAsync(transaction, cancellationToken);
        
        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        transaction.Complete();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        transaction = await _context.Set<SellerTransaction>()
            .AsNoTracking()
            .Include(t => t.Seller)
            .FirstOrDefaultAsync(t => t.Id == transaction.Id, cancellationToken);

        _logger.LogInformation("Transaction created. TransactionId: {TransactionId}", transaction!.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<SellerTransactionDto>(transaction);
    }
}
