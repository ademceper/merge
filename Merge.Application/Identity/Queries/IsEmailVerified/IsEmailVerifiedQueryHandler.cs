using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Merge.Domain.Entities;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Identity.Queries.IsEmailVerified;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class IsEmailVerifiedQueryHandler : IRequestHandler<IsEmailVerifiedQuery, bool>
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly ILogger<IsEmailVerifiedQueryHandler> _logger;

    public IsEmailVerifiedQueryHandler(
        UserManager<UserEntity> userManager,
        ILogger<IsEmailVerifiedQueryHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<bool> Handle(IsEmailVerifiedQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking email verification status. UserId: {UserId}", request.UserId);

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        var isVerified = user?.EmailConfirmed ?? false;

        _logger.LogInformation("Email verification status checked. UserId: {UserId}, IsVerified: {IsVerified}", request.UserId, isVerified);
        return isVerified;
    }
}

