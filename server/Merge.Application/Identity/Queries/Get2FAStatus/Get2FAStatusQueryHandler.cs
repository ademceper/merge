using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Identity;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Identity.Queries.Get2FAStatus;

public class Get2FAStatusQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<Get2FAStatusQueryHandler> logger) : IRequestHandler<Get2FAStatusQuery, TwoFactorStatusDto?>
{

    public async Task<TwoFactorStatusDto?> Handle(Get2FAStatusQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting 2FA status. UserId: {UserId}", request.UserId);

        var twoFactorAuth = await context.Set<TwoFactorAuth>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (twoFactorAuth is null)
        {
            return new TwoFactorStatusDto(
                IsEnabled: false,
                Method: TwoFactorMethod.None,
                PhoneNumber: null,
                Email: null,
                BackupCodesRemaining: 0);
        }

        var dto = mapper.Map<TwoFactorStatusDto>(twoFactorAuth);
        return new TwoFactorStatusDto(
            IsEnabled: dto.IsEnabled,
            Method: dto.Method,
            PhoneNumber: twoFactorAuth.PhoneNumber is not null ? MaskPhoneNumber(twoFactorAuth.PhoneNumber) : null,
            Email: twoFactorAuth.Email is not null ? MaskEmail(twoFactorAuth.Email) : null,
            BackupCodesRemaining: twoFactorAuth.BackupCodes?.Length ?? 0);
    }

    private string MaskPhoneNumber(string phone)
    {
        if (phone.Length < 4) return phone;
        var lastFour = phone.AsSpan(phone.Length - 4);
        return $"***{lastFour}";
    }

    private string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex >= email.Length - 1) return email;
        
        var usernameSpan = email.AsSpan(0, atIndex);
        if (usernameSpan.Length <= 2) return email;
        
        var domainSpan = email.AsSpan(atIndex);
        return $"{usernameSpan[0]}***{usernameSpan[usernameSpan.Length - 1]}{domainSpan}";
    }
}

