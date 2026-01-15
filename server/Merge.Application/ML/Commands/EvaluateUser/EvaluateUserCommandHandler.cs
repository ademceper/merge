using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.ML.Helpers;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Commands.EvaluateUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class EvaluateUserCommandHandler : IRequestHandler<EvaluateUserCommand, FraudAlertDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<EvaluateUserCommandHandler> _logger;
    private readonly FraudDetectionHelper _helper;

    public EvaluateUserCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<EvaluateUserCommandHandler> logger,
        FraudDetectionHelper helper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _helper = helper;
    }

    public async Task<FraudAlertDto> Handle(EvaluateUserCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Evaluating user for fraud. UserId: {UserId}", request.UserId);

        // ✅ PERFORMANCE: Removed manual !u.IsDeleted (Global Query Filter)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        // ✅ BOLUM 1.2: Enum kullanımı (string YASAK)
        var riskScore = await _helper.CalculateRiskScoreAsync(FraudRuleType.Account, null, request.UserId, cancellationToken);
        var matchedRules = await _helper.GetMatchedRulesAsync(FraudRuleType.Account, null, request.UserId, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var matchedRulesJson = matchedRules.Any() ? JsonSerializer.Serialize(matchedRules.Select(r => r.Id)) : null;
        var alert = FraudAlert.Create(
            userId: request.UserId,
            alertType: FraudAlertType.Account,
            riskScore: riskScore,
            reason: $"User evaluation: Risk score {riskScore}",
            matchedRules: matchedRulesJson);

        await _context.Set<FraudAlert>().AddAsync(alert, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var createdAlert = await _context.Set<FraudAlert>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.Order)
            .Include(a => a.Payment)
            .Include(a => a.ReviewedBy)
            .FirstOrDefaultAsync(a => a.Id == alert.Id, cancellationToken);

        _logger.LogInformation("User evaluated. UserId: {UserId}, AlertId: {AlertId}, RiskScore: {RiskScore}",
            request.UserId, alert.Id, alert.RiskScore);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FraudAlertDto>(createdAlert!);
    }
}
