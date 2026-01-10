using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;

namespace Merge.Application.ML.Queries.GetFraudDetectionRuleById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetFraudDetectionRuleByIdQueryHandler : IRequestHandler<GetFraudDetectionRuleByIdQuery, FraudDetectionRuleDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFraudDetectionRuleByIdQueryHandler> _logger;

    public GetFraudDetectionRuleByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetFraudDetectionRuleByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FraudDetectionRuleDto?> Handle(GetFraudDetectionRuleByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting fraud detection rule by ID. RuleId: {RuleId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var rule = await _context.Set<Merge.Domain.Entities.FraudDetectionRule>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rule == null)
        {
            _logger.LogWarning("Fraud detection rule not found. RuleId: {RuleId}", request.Id);
            return null;
        }

        _logger.LogInformation("Fraud detection rule retrieved. RuleId: {RuleId}, Name: {Name}",
            rule.Id, rule.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FraudDetectionRuleDto>(rule);
    }
}
