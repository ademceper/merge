using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;

namespace Merge.Application.B2B.Queries.GetB2BUserById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetB2BUserByIdQueryHandler : IRequestHandler<GetB2BUserByIdQuery, B2BUserDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetB2BUserByIdQueryHandler> _logger;

    public GetB2BUserByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetB2BUserByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<B2BUserDto?> Handle(GetB2BUserByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !b.IsDeleted check (Global Query Filter handles it)
        var b2bUser = await _context.Set<B2BUser>()
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return b2bUser != null ? _mapper.Map<B2BUserDto>(b2bUser) : null;
    }
}

