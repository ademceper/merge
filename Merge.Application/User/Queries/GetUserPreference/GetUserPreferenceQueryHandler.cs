using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Queries.GetUserPreference;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetUserPreferenceQueryHandler : IRequestHandler<GetUserPreferenceQuery, UserPreferenceDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserPreferenceQueryHandler> _logger;

    public GetUserPreferenceQueryHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetUserPreferenceQueryHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserPreferenceDto> Handle(GetUserPreferenceQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving preferences for user: {UserId}", request.UserId);

        var preferences = await _context.Set<UserPreference>()
            .AsNoTracking()
            .Where(up => up.UserId == request.UserId && !up.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (preferences == null)
        {
            _logger.LogInformation("No preferences found for user: {UserId}, creating default preferences", request.UserId);

            preferences = UserPreference.Create(request.UserId);
            await _context.Set<UserPreference>().AddAsync(preferences, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Default preferences created for user: {UserId}", request.UserId);
        }

        return _mapper.Map<UserPreferenceDto>(preferences);
    }
}
