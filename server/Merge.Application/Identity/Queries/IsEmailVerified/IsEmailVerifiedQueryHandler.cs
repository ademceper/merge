using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Merge.Domain.Entities;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Identity.Queries.IsEmailVerified;

public class IsEmailVerifiedQueryHandler(
    UserManager<UserEntity> userManager,
    ILogger<IsEmailVerifiedQueryHandler> logger) : IRequestHandler<IsEmailVerifiedQuery, bool>
{
    public async Task<bool> Handle(IsEmailVerifiedQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking email verification status. UserId: {UserId}", request.UserId);

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        var isVerified = user?.EmailConfirmed ?? false;

        logger.LogInformation("Email verification status checked. UserId: {UserId}, IsVerified: {IsVerified}", request.UserId, isVerified);
        return isVerified;
    }
}

