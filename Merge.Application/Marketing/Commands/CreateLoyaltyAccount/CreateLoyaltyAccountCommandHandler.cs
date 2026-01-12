using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.CreateLoyaltyAccount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateLoyaltyAccountCommandHandler : IRequestHandler<CreateLoyaltyAccountCommand, LoyaltyAccountDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateLoyaltyAccountCommandHandler> _logger;
    private readonly LoyaltySettings _loyaltySettings;

    public CreateLoyaltyAccountCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateLoyaltyAccountCommandHandler> logger,
        IOptions<LoyaltySettings> loyaltySettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _loyaltySettings = loyaltySettings.Value;
    }

    public async Task<LoyaltyAccountDto> Handle(CreateLoyaltyAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating loyalty account. UserId: {UserId}", request.UserId);

        // ✅ PERFORMANCE: AsNoTracking - Check if account already exists
        var exists = await _context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .AnyAsync(a => a.UserId == request.UserId, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Loyalty account already exists. UserId: {UserId}", request.UserId);
            throw new BusinessException("Sadakat hesabı zaten mevcut.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var account = LoyaltyAccount.Create(request.UserId);

        await _context.Set<LoyaltyAccount>().AddAsync(account, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan
        // Signup bonus points ekle
        account.AddPoints(_loyaltySettings.SignupBonusPoints, "Signup bonus");
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + AsSplitQuery ile tek query'de getir (N+1 query önleme)
        var createdAccount = await _context.Set<LoyaltyAccount>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Tier)
            .FirstOrDefaultAsync(a => a.Id == account.Id, cancellationToken);

        if (createdAccount == null)
        {
            _logger.LogWarning("LoyaltyAccount not found after creation. AccountId: {AccountId}", account.Id);
            throw new NotFoundException("Sadakat hesabı", account.Id);
        }

        _logger.LogInformation("LoyaltyAccount created successfully. AccountId: {AccountId}, UserId: {UserId}, SignupBonusPoints: {SignupBonusPoints}", 
            account.Id, request.UserId, _loyaltySettings.SignupBonusPoints);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<LoyaltyAccountDto>(createdAccount);
    }
}
