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

public class EvaluateUserCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<EvaluateUserCommandHandler> logger, FraudDetectionHelper helper) : IRequestHandler<EvaluateUserCommand, FraudAlertDto>
{

    public async Task<FraudAlertDto> Handle(EvaluateUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Evaluating user for fraud. UserId: {UserId}", request.UserId);

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("User not found. UserId: {UserId}", request.UserId);
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        var riskScore = await helper.CalculateRiskScoreAsync(FraudRuleType.Account, null, request.UserId, cancellationToken);
        var matchedRules = await helper.GetMatchedRulesAsync(FraudRuleType.Account, null, request.UserId, cancellationToken);

        var matchedRulesJson = matchedRules.Any() ? JsonSerializer.Serialize(matchedRules.Select(r => r.Id)) : null;
        var alert = FraudAlert.Create(
            userId: request.UserId,
            alertType: FraudAlertType.Account,
            riskScore: riskScore,
            reason: $"User evaluation: Risk score {riskScore}",
            matchedRules: matchedRulesJson);

        await context.Set<FraudAlert>().AddAsync(alert, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdAlert = await context.Set<FraudAlert>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Order)
            .Include(a => a.Payment)
            .Include(a => a.ReviewedBy)
            .FirstOrDefaultAsync(a => a.Id == alert.Id, cancellationToken);

        logger.LogInformation("User evaluated. UserId: {UserId}, AlertId: {AlertId}, RiskScore: {RiskScore}",
            request.UserId, alert.Id, alert.RiskScore);

        return mapper.Map<FraudAlertDto>(createdAlert!);
    }
}
