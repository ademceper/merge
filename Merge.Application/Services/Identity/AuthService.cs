using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Merge.Application.DTOs.Auth;
using Merge.Application.Interfaces.Identity;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using UserEntity = Merge.Domain.Entities.User;
using Merge.Application.DTOs.User;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Microsoft.Extensions.Logging;


namespace Merge.Application.Services.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IMapper _mapper;

    // ✅ SECURITY: Access token süresi 15 dakika (önceki: 24 saat)
    private const int AccessTokenExpirationMinutes = 15;
    // ✅ SECURITY: Refresh token süresi 7 gün
    private const int RefreshTokenExpirationDays = 7;

    public AuthService(
        UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager,
        RoleManager<Role> roleManager,
        IDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IMapper mapper)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        if (registerDto == null)
        {
            throw new ArgumentNullException(nameof(registerDto));
        }

        if (string.IsNullOrWhiteSpace(registerDto.Email))
        {
            throw new ValidationException("Email adresi boş olamaz.");
        }

        if (string.IsNullOrWhiteSpace(registerDto.Password))
        {
            throw new ValidationException("Şifre boş olamaz.");
        }

        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            throw new BusinessException("Bu email adresi zaten kullanılıyor.");
        }

        var user = new UserEntity
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            UserName = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            throw new ValidationException("Kullanıcı oluşturulamadı.", errors);
        }

        // Default role assignment
        await _userManager.AddToRoleAsync(user, "Customer");

        var roles = await _userManager.GetRolesAsync(user);

        _logger.LogInformation(
            "User registered successfully. UserId: {UserId}, Email: {Email}",
            user.Id, registerDto.Email);

        return await GenerateAuthResponseAsync(user, roles, null, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        if (loginDto == null)
        {
            throw new ArgumentNullException(nameof(loginDto));
        }

        if (string.IsNullOrWhiteSpace(loginDto.Email))
        {
            throw new ValidationException("Email adresi boş olamaz.");
        }

        if (string.IsNullOrWhiteSpace(loginDto.Password))
        {
            throw new ValidationException("Şifre boş olamaz.");
        }

        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            throw new BusinessException("Email veya şifre hatalı.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out. Email: {Email}", loginDto.Email);
                throw new BusinessException("Hesabınız geçici olarak kilitlendi. Lütfen daha sonra tekrar deneyin.");
            }
            _logger.LogWarning("Failed login attempt. Email: {Email}", loginDto.Email);
            throw new BusinessException("Email veya şifre hatalı.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        _logger.LogInformation(
            "User logged in successfully. UserId: {UserId}, Email: {Email}",
            user.Id, loginDto.Email);

        return await GenerateAuthResponseAsync(user, roles, null, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.1: Refresh token hash'lenmiş olarak saklanıyor
        var tokenHash = TokenHasher.HashToken(refreshToken);
        var refreshTokenEntity = await _context.Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (refreshTokenEntity == null)
        {
            throw new BusinessException("Geçersiz refresh token.");
        }

        if (!refreshTokenEntity.IsActive)
        {
            throw new BusinessException("Refresh token geçersiz veya süresi dolmuş.");
        }

        var user = refreshTokenEntity.User;
        var roles = await _userManager.GetRolesAsync(user);

        // ✅ SECURITY: Eski refresh token'ı revoke et
        refreshTokenEntity.IsRevoked = true;
        refreshTokenEntity.RevokedAt = DateTime.UtcNow;
        refreshTokenEntity.RevokedByIp = ipAddress;

        // ✅ SECURITY: Yeni refresh token oluştur (rotation)
        var (newRefreshToken, plainToken) = GenerateRefreshToken(user.Id, ipAddress);
        refreshTokenEntity.ReplacedByTokenHash = newRefreshToken.TokenHash;

        _context.Set<RefreshToken>().Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Token refreshed successfully. UserId: {UserId}",
            user.Id);

        var accessToken = await GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Role = roles.Count > 0 ? roles[0] : "Customer";

        return new AuthResponseDto
        {
            Token = accessToken,
            ExpiresAt = expiresAt,
            RefreshToken = plainToken,
            RefreshTokenExpiresAt = newRefreshToken.ExpiresAt,
            User = userDto
        };
    }

    public async Task RevokeTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.1: Refresh token hash'lenmiş olarak saklanıyor
        var tokenHash = TokenHasher.HashToken(refreshToken);
        var refreshTokenEntity = await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (refreshTokenEntity == null)
        {
            throw new BusinessException("Geçersiz refresh token.");
        }

        if (!refreshTokenEntity.IsActive)
        {
            throw new BusinessException("Refresh token zaten geçersiz.");
        }

        refreshTokenEntity.IsRevoked = true;
        refreshTokenEntity.RevokedAt = DateTime.UtcNow;
        refreshTokenEntity.RevokedByIp = ipAddress;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Token revoked successfully. UserId: {UserId}",
            refreshTokenEntity.UserId);
    }

    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key bulunamadı"));

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return Task.FromResult(false);
        }
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(UserEntity user, IList<string> roles, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var accessToken = await GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes);

        var (refreshToken, plainToken) = GenerateRefreshToken(user.Id, ipAddress);
        _context.Set<RefreshToken>().Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Role = roles.Count > 0 ? roles[0] : "Customer";

        return new AuthResponseDto
        {
            Token = accessToken,
            ExpiresAt = expiresAt,
            RefreshToken = plainToken,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            User = userDto
        };
    }

    // ✅ BOLUM 9.1: Refresh token hash'lenmiş olarak saklanıyor
    // Tuple döndürüyor: (RefreshToken entity, plain token string)
    private (RefreshToken, string) GenerateRefreshToken(Guid userId, string? ipAddress = null)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        
        var plainToken = Convert.ToBase64String(randomBytes);
        var tokenHash = TokenHasher.HashToken(plainToken);

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays),
            CreatedByIp = ipAddress
        };
        
        return (refreshToken, plainToken);
    }

    private async Task<string> GenerateJwtToken(UserEntity user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key bulunamadı"));

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add roles to claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            // ✅ SECURITY: Access token süresi 15 dakika
            Expires = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
