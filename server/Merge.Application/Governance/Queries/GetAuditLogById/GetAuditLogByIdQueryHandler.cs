using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Queries.GetAuditLogById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAuditLogByIdQueryHandler : IRequestHandler<GetAuditLogByIdQuery, AuditLogDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAuditLogByIdQueryHandler> _logger;

    public GetAuditLogByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAuditLogByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuditLogDto?> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving audit log with Id: {AuditLogId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var audit = await _context.Set<AuditLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (audit == null)
        {
            _logger.LogWarning("Audit log not found. AuditLogId: {AuditLogId}", request.Id);
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<AuditLogDto>(audit);
    }
}
