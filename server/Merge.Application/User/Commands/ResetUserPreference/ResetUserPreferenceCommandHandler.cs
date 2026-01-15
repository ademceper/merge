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

namespace Merge.Application.User.Commands.ResetUserPreference;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ResetUserPreferenceCommandHandler : IRequestHandler<ResetUserPreferenceCommand, UserPreferenceDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ResetUserPreferenceCommandHandler> _logger;

    public ResetUserPreferenceCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ResetUserPreferenceCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserPreferenceDto> Handle(ResetUserPreferenceCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)

        _logger.LogInformation("Resetting preferences to defaults for user: {UserId}", request.UserId);

        var preferences = await _context.Set<UserPreference>()
            .Where(up => up.UserId == request.UserId && !up.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (preferences == null)
        {
            _logger.LogInformation("No preferences found for user: {UserId}, creating default preferences", request.UserId);
            preferences = UserPreference.Create(request.UserId);
            await _context.Set<UserPreference>().AddAsync(preferences, cancellationToken);
        }
        else
        {
                        preferences.ResetToDefaults();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event\'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır

        _logger.LogInformation("Preferences reset to defaults successfully for user: {UserId}", request.UserId);

                // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<UserPreferenceDto>(preferences);
    }
}
