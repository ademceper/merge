using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Queries.GetB2BUserByUserId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetB2BUserByUserIdQueryHandler : IRequestHandler<GetB2BUserByUserIdQuery, B2BUserDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetB2BUserByUserIdQueryHandler> _logger;

    public GetB2BUserByUserIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetB2BUserByUserIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<B2BUserDto?> Handle(GetB2BUserByUserIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !b.IsDeleted check (Global Query Filter handles it)
        var b2bUser = await _context.Set<B2BUser>()
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.UserId == request.UserId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return b2bUser != null ? _mapper.Map<B2BUserDto>(b2bUser) : null;
    }
}

