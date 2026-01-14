using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetSellerApplication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSellerApplicationQueryHandler : IRequestHandler<GetSellerApplicationQuery, SellerApplicationDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSellerApplicationQueryHandler> _logger;

    public GetSellerApplicationQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetSellerApplicationQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SellerApplicationDto?> Handle(GetSellerApplicationQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting seller application. ApplicationId: {ApplicationId}", request.ApplicationId);

        var application = await _context.Set<SellerApplication>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Reviewer)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return application == null ? null : _mapper.Map<SellerApplicationDto>(application);
    }
}
