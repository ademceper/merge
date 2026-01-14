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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class Get2FAStatusQueryHandler : IRequestHandler<Get2FAStatusQuery, TwoFactorStatusDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<Get2FAStatusQueryHandler> _logger;

    public Get2FAStatusQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<Get2FAStatusQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TwoFactorStatusDto?> Handle(Get2FAStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting 2FA status. UserId: {UserId}", request.UserId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var twoFactorAuth = await _context.Set<TwoFactorAuth>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (twoFactorAuth == null)
        {
            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return new TwoFactorStatusDto(
                IsEnabled: false,
                Method: TwoFactorMethod.None,
                PhoneNumber: null,
                Email: null,
                BackupCodesRemaining: 0);
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<TwoFactorStatusDto>(twoFactorAuth);
        // ✅ PERFORMANCE: Memory'de minimal işlem (sadece property assignment)
        // Record'lar immutable olduğu için yeni instance oluşturmamız gerekiyor
        return new TwoFactorStatusDto(
            IsEnabled: dto.IsEnabled,
            Method: dto.Method,
            PhoneNumber: twoFactorAuth.PhoneNumber != null ? MaskPhoneNumber(twoFactorAuth.PhoneNumber) : null,
            Email: twoFactorAuth.Email != null ? MaskEmail(twoFactorAuth.Email) : null,
            BackupCodesRemaining: twoFactorAuth.BackupCodes?.Length ?? 0);
    }

    // ✅ PERFORMANCE: String operations optimized using Span<char>
    private string MaskPhoneNumber(string phone)
    {
        if (phone.Length < 4) return phone;
        // ✅ PERFORMANCE: Use AsSpan for substring operations (zero allocation)
        var lastFour = phone.AsSpan(phone.Length - 4);
        return $"***{lastFour}";
    }

    // ✅ PERFORMANCE: String operations optimized using Span<char>
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

