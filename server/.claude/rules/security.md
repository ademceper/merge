---
paths:
  - "**/*.cs"
  - "**/appsettings*.json"
  - "**/*.config"
alwaysApply: true
---

# SECURITY RULES - OWASP TOP 10 COMPLIANCE

> Bu dosya güvenlik kurallarını içerir.
> CLAUDE: Bu kurallara MUTLAKA uy! Güvenlik ihlali ASLA kabul edilemez!

---

## 1. A01:2021 - BROKEN ACCESS CONTROL

### 1.1 Authorization (ZORUNLU)

```csharp
// ✅ DOĞRU: Tüm controller'larda [Authorize]
[ApiController]
[Route("api/v1/orders")]
[Authorize] // DEFAULT: Authenticated users only
public class OrdersController : BaseController
{
    [HttpGet]
    [AllowAnonymous] // Explicitly allow public
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetPublicOrders() { }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id) { }

    [HttpPost]
    [Authorize(Roles = "Customer,Admin")] // Role-based
    public async Task<ActionResult<OrderDto>> Create() { }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdminRole")] // Policy-based
    public async Task<IActionResult> Delete(Guid id) { }
}

// ❌ YANLIŞ: [Authorize] yok
[ApiController]
[Route("api/v1/orders")]
public class OrdersController : BaseController // YANLIŞ!
{
}
```

### 1.2 IDOR Protection (CRITICAL)

```csharp
// ✅ DOĞRU: Resource ownership verification
[HttpGet("{orderId}")]
public async Task<ActionResult<OrderDto>> GetOrder(Guid orderId, CancellationToken ct)
{
    var userId = GetCurrentUserId(); // From JWT claims
    var order = await mediator.Send(new GetOrderByIdQuery(orderId), ct);

    if (order is null)
        return NotFound();

    // CRITICAL: Verify ownership
    if (order.UserId != userId && !User.IsInRole("Admin"))
        return Forbid();

    return Ok(order);
}

// ✅ DOĞRU: Query level filtering
public class GetUserOrdersQueryHandler(
    IRepository<Order> repository,
    ICurrentUserService currentUser) : IRequestHandler<GetUserOrdersQuery, List<OrderDto>>
{
    public async Task<List<OrderDto>> Handle(GetUserOrdersQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId;

        // User can ONLY see their own orders
        var spec = new OrdersByUserIdSpec(userId);
        var orders = await repository.ListAsync(spec, ct);

        return mapper.Map<List<OrderDto>>(orders);
    }
}

// ❌ YANLIŞ: No ownership check
[HttpGet("{orderId}")]
public async Task<ActionResult<OrderDto>> GetOrder(Guid orderId, CancellationToken ct)
{
    var order = await mediator.Send(new GetOrderByIdQuery(orderId), ct);
    return Ok(order); // ANYONE can see ANY order!
}
```

### 1.3 Horizontal Privilege Escalation

```csharp
// ✅ DOĞRU: Seller can only manage own products
public class UpdateProductCommandHandler(
    IRepository<Product> repository,
    ICurrentUserService currentUser) : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Product", request.Id);

        // Verify seller owns this product
        var userId = currentUser.UserId;
        var userRole = currentUser.Role;

        if (product.SellerId != userId && userRole != "Admin")
            throw new UnauthorizedException("You can only update your own products");

        product.Update(request.Name, request.Price);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<ProductDto>(product);
    }
}
```

### 1.4 Vertical Privilege Escalation

```csharp
// ✅ DOĞRU: Admin-only operations
public class DeleteUserCommandHandler(
    IRepository<User> repository,
    ICurrentUserService currentUser) : IRequestHandler<DeleteUserCommand, bool>
{
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        // Only admins can delete users
        if (currentUser.Role != "Admin")
            throw new UnauthorizedException("Only admins can delete users");

        // Admins cannot delete themselves
        if (request.UserId == currentUser.UserId)
            throw new DomainException("You cannot delete your own account");

        // Admins cannot delete other admins (only SuperAdmin can)
        var targetUser = await repository.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        if (targetUser.Role == "Admin" && currentUser.Role != "SuperAdmin")
            throw new UnauthorizedException("Only SuperAdmin can delete Admin accounts");

        targetUser.Delete();
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
```

---

## 2. A02:2021 - CRYPTOGRAPHIC FAILURES

### 2.1 Password Hashing (BCrypt ONLY)

```csharp
// ✅ DOĞRU: BCrypt for passwords
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12; // 2^12 iterations

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}

// ❌ YASAK: Weak hashing
using var md5 = MD5.Create();        // BROKEN!
using var sha1 = SHA1.Create();      // BROKEN!
var hash = SHA256.Create();          // For passwords, use BCrypt instead!
```

### 2.2 Token Hashing (SHA256)

```csharp
// ✅ DOĞRU: SHA256 for tokens
public class TokenService : ITokenService
{
    public string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
```

### 2.3 HMAC for Signatures

```csharp
// ✅ DOĞRU: HMACSHA256 for signatures
public class SignatureService : ISignatureService
{
    private readonly byte[] _key;

    public SignatureService(IOptions<SecuritySettings> settings)
    {
        _key = Encoding.UTF8.GetBytes(settings.Value.HmacKey);
    }

    public string Sign(string data)
    {
        using var hmac = new HMACSHA256(_key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }

    public bool Verify(string data, string signature)
    {
        var expectedSignature = Sign(data);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSignature),
            Encoding.UTF8.GetBytes(signature));
    }
}
```

### 2.4 Sensitive Data Encryption

```csharp
// ✅ DOĞRU: AES-256 for data at rest
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Prepend IV to ciphertext
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        aes.IV.CopyTo(result, 0);
        cipherBytes.CopyTo(result, aes.IV.Length);

        return Convert.ToBase64String(result);
    }
}
```

---

## 3. A03:2021 - INJECTION

### 3.1 SQL Injection Prevention

```csharp
// ✅ DOĞRU: Parameterized queries (EF Core handles this)
public async Task<List<Product>> SearchAsync(string searchTerm, CancellationToken ct)
{
    return await context.Products
        .Where(p => EF.Functions.Like(p.Name, $"%{searchTerm}%"))
        .ToListAsync(ct);
}

// ✅ DOĞRU: Raw SQL with parameters
public async Task<List<Product>> SearchRawAsync(string searchTerm, CancellationToken ct)
{
    return await context.Products
        .FromSqlInterpolated($"""
            SELECT * FROM "Products"
            WHERE "Name" ILIKE '%' || {searchTerm} || '%'
        """)
        .ToListAsync(ct);
}

// ❌ YASAK: String concatenation
public async Task<List<Product>> SearchAsync(string searchTerm, CancellationToken ct)
{
    return await context.Products
        .FromSqlRaw($"SELECT * FROM Products WHERE Name LIKE '%{searchTerm}%'") // SQL INJECTION!
        .ToListAsync(ct);
}
```

### 3.2 Command Injection Prevention

```csharp
// ✅ DOĞRU: Validate and sanitize
public async Task<string> GenerateReportAsync(string reportType, CancellationToken ct)
{
    // Whitelist allowed values
    var allowedTypes = new[] { "sales", "inventory", "customers" };
    if (!allowedTypes.Contains(reportType.ToLowerInvariant()))
        throw new ValidationException("Invalid report type");

    // Use parameterized approach
    var report = await reportService.GenerateAsync(reportType, ct);
    return report;
}

// ❌ YASAK: Executing user input
public async Task<string> ExecuteCommandAsync(string command)
{
    var process = Process.Start("cmd", $"/c {command}"); // COMMAND INJECTION!
    return await process.StandardOutput.ReadToEndAsync();
}
```

### 3.3 LDAP Injection Prevention

```csharp
// ✅ DOĞRU: Escape LDAP special characters
public string EscapeLdap(string input)
{
    if (string.IsNullOrEmpty(input))
        return input;

    var sb = new StringBuilder();
    foreach (char c in input)
    {
        switch (c)
        {
            case '\\': sb.Append("\\5c"); break;
            case '*': sb.Append("\\2a"); break;
            case '(': sb.Append("\\28"); break;
            case ')': sb.Append("\\29"); break;
            case '\0': sb.Append("\\00"); break;
            default: sb.Append(c); break;
        }
    }
    return sb.ToString();
}
```

---

## 4. A04:2021 - INSECURE DESIGN

### 4.1 Defense in Depth

```csharp
// Multiple layers of validation
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // Layer 1: FluentValidation (input validation)
        // Automatically run by ValidationBehavior

        // Layer 2: Authorization check
        var userId = currentUser.UserId;
        if (request.UserId != userId && !currentUser.IsAdmin)
            throw new UnauthorizedException();

        // Layer 3: Business rule validation
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        if (user.Status == UserStatus.Suspended)
            throw new DomainException("Suspended users cannot place orders");

        // Layer 4: Domain validation (in entity)
        var order = Order.Create(userId, request.ShippingAddress);
        // Order.Create validates invariants internally

        // Layer 5: Database constraints
        // Unique indexes, foreign keys, check constraints
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<OrderDto>(order);
    }
}
```

### 4.2 Rate Limiting

```csharp
// ✅ DOĞRU: Rate limiting on sensitive endpoints
[HttpPost("login")]
[AllowAnonymous]
[EnableRateLimiting("login")] // 5 requests per minute
public async Task<ActionResult<LoginResponse>> Login(LoginCommand command, CancellationToken ct)
{
    return Ok(await mediator.Send(command, ct));
}

[HttpPost("forgot-password")]
[AllowAnonymous]
[EnableRateLimiting("forgot-password")] // 3 requests per 5 minutes
public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command, CancellationToken ct)
{
    await mediator.Send(command, ct);
    return Ok();
}

[HttpPost("verify-email")]
[AllowAnonymous]
[EnableRateLimiting("email-verification")] // 10 requests per hour
public async Task<IActionResult> VerifyEmail(VerifyEmailCommand command, CancellationToken ct)
{
    await mediator.Send(command, ct);
    return Ok();
}

// Rate limiting configuration
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("forgot-password", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromMinutes(5);
        opt.QueueLimit = 0;
    });

    options.AddSlidingWindowLimiter("api", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6;
        opt.QueueLimit = 10;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
```

### 4.3 Account Lockout

```csharp
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, ct);

        if (user is null)
        {
            // Don't reveal if email exists - timing attack prevention
            await Task.Delay(Random.Shared.Next(100, 300), ct);
            throw new UnauthorizedException("Invalid credentials");
        }

        // Check lockout
        if (user.IsLockedOut && user.LockoutEnd > DateTime.UtcNow)
        {
            var remainingMinutes = (user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes;
            throw new UnauthorizedException($"Account locked. Try again in {remainingMinutes:F0} minutes");
        }

        // Verify password
        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();

            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockAccount(DateTime.UtcNow.AddMinutes(LockoutMinutes));
                logger.LogWarning("Account locked due to failed attempts: {UserId}", user.Id);
            }

            await unitOfWork.SaveChangesAsync(ct);
            throw new UnauthorizedException("Invalid credentials");
        }

        // Successful login
        user.RecordSuccessfulLogin();
        await unitOfWork.SaveChangesAsync(ct);

        return new LoginResponse(
            tokenService.GenerateAccessToken(user),
            tokenService.GenerateRefreshToken());
    }
}
```

---

## 5. A05:2021 - SECURITY MISCONFIGURATION

### 5.1 No Hardcoded Secrets (CRITICAL)

```csharp
// ❌ YASAK: Hardcoded secrets
public class AuthService
{
    private const string JwtSecret = "MySecretKey123!"; // YASAK!
    private const string DbPassword = "postgres123";     // YASAK!
    private const string ApiKey = "sk-abc123xyz";        // YASAK!
}

// ✅ DOĞRU: Environment variables / Configuration
public class AuthService(IOptions<JwtSettings> jwtSettings)
{
    private readonly JwtSettings _settings = jwtSettings.Value;

    public string GenerateToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_settings.SecretKey);
        // ...
    }
}

// appsettings.json (with placeholders)
{
    "JwtSettings": {
        "SecretKey": "${JWT_SECRET_KEY}",
        "Issuer": "${JWT_ISSUER}",
        "Audience": "${JWT_AUDIENCE}"
    }
}
```

### 5.2 Security Headers

```csharp
// Program.cs - Add security headers
app.Use(async (context, next) =>
{
    // Prevent MIME type sniffing
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

    // Prevent clickjacking
    context.Response.Headers["X-Frame-Options"] = "DENY";

    // Enable XSS filter (legacy browsers)
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

    // Control referrer information
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    // Permissions policy
    context.Response.Headers["Permissions-Policy"] =
        "geolocation=(), microphone=(), camera=()";

    // Content Security Policy (adjust as needed)
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'";

    await next();
});
```

### 5.3 CORS Configuration

```csharp
// Development - Allow all (acceptable for dev)
if (app.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Development", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
}

// Production - Whitelist only
else
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Production", policy =>
        {
            policy.WithOrigins(
                    "https://merge.com",
                    "https://www.merge.com",
                    "https://admin.merge.com")
                  .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
                  .WithHeaders("Authorization", "Content-Type", "X-Requested-With")
                  .AllowCredentials()
                  .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
        });
    });
}
```

### 5.4 HTTPS Enforcement

```csharp
// Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

// Force HTTPS in production
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
```

---

## 6. A06:2021 - VULNERABLE COMPONENTS

### 6.1 Dependency Management

```bash
# Güvenlik açıkları için tarama
dotnet list package --vulnerable --include-transitive

# Güncellemeleri kontrol et
dotnet list package --outdated

# NuGet audit (dotnet 8+)
dotnet restore --force-evaluate
```

### 6.2 Package Versions

```xml
<!-- ✅ DOĞRU: Fixed versions -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
<PackageReference Include="FluentValidation" Version="11.9.2" />

<!-- ❌ YANLIŞ: Floating versions (security risk) -->
<PackageReference Include="SomePackage" Version="*" />
<PackageReference Include="SomePackage" Version="1.*" />
```

---

## 7. A07:2021 - IDENTIFICATION AND AUTHENTICATION FAILURES

### 7.1 JWT Token Security

```csharp
public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("TenantId", user.TenantId?.ToString() ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes), // Short-lived: 15-30 min
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

// JWT Validation
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero // No tolerance for expiration
        };
    });
```

### 7.2 Refresh Token Security

```csharp
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        // 1. Hash the incoming token
        var tokenHash = tokenService.HashToken(request.RefreshToken);

        // 2. Find token in database
        var refreshToken = await refreshTokenRepository.GetByHashAsync(tokenHash, ct);

        if (refreshToken is null)
        {
            logger.LogWarning("Refresh token not found (possible reuse attack)");
            throw new UnauthorizedException("Invalid refresh token");
        }

        // 3. Validate token
        if (refreshToken.IsExpired)
        {
            await refreshTokenRepository.DeleteAsync(refreshToken, ct);
            throw new UnauthorizedException("Refresh token expired");
        }

        if (refreshToken.IsRevoked)
        {
            // Possible token theft - revoke all tokens for this user
            logger.LogWarning("Revoked token reuse detected for user: {UserId}", refreshToken.UserId);
            await refreshTokenRepository.RevokeAllForUserAsync(refreshToken.UserId, ct);
            throw new UnauthorizedException("Invalid refresh token");
        }

        // 4. Get user
        var user = await userRepository.GetByIdAsync(refreshToken.UserId, ct)
            ?? throw new UnauthorizedException("User not found");

        if (user.Status != UserStatus.Active)
            throw new UnauthorizedException("User account is not active");

        // 5. Rotate refresh token (one-time use)
        refreshToken.Revoke();

        var newRefreshToken = RefreshToken.Create(
            user.Id,
            tokenService.GenerateRefreshToken(),
            DateTime.UtcNow.AddDays(7));

        await refreshTokenRepository.AddAsync(newRefreshToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        // 6. Generate new tokens
        return new TokenResponse(
            tokenService.GenerateAccessToken(user),
            newRefreshToken.Token);
    }
}
```

### 7.3 Password Requirements

```csharp
public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(12).WithMessage("Password must be at least 12 characters")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character")
            .Must(NotContainCommonPatterns).WithMessage("Password contains common patterns");
    }

    private bool NotContainCommonPatterns(string password)
    {
        if (string.IsNullOrEmpty(password))
            return true;

        var commonPatterns = new[]
        {
            "password", "123456", "qwerty", "abc123", "admin",
            "letmein", "welcome", "monkey", "dragon", "master"
        };

        var lowerPassword = password.ToLowerInvariant();
        return !commonPatterns.Any(p => lowerPassword.Contains(p));
    }
}
```

---

## 8. A08:2021 - SOFTWARE AND DATA INTEGRITY FAILURES

### 8.1 Webhook Signature Verification

```csharp
// ✅ DOĞRU: Verify webhook signatures
[HttpPost("webhooks/stripe")]
[AllowAnonymous]
public async Task<IActionResult> HandleStripeWebhook()
{
    var json = await new StreamReader(Request.Body).ReadToEndAsync();
    var signature = Request.Headers["Stripe-Signature"].ToString();

    try
    {
        var stripeEvent = EventUtility.ConstructEvent(
            json,
            signature,
            _settings.StripeWebhookSecret,
            throwOnApiVersionMismatch: false);

        // Process webhook
        await ProcessStripeEventAsync(stripeEvent);

        return Ok();
    }
    catch (StripeException ex)
    {
        logger.LogWarning(ex, "Invalid Stripe webhook signature");
        return BadRequest("Invalid signature");
    }
}

// ✅ DOĞRU: Custom webhook signature verification
public class WebhookSignatureValidator : IWebhookSignatureValidator
{
    public bool Validate(string payload, string signature, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var expectedSignature = Convert.ToBase64String(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSignature),
            Encoding.UTF8.GetBytes(signature));
    }
}
```

### 8.2 Anti-Tampering

```csharp
// ✅ DOĞRU: Validate data integrity
public class OrderIntegrityService : IOrderIntegrityService
{
    public string GenerateChecksum(Order order)
    {
        var data = $"{order.Id}|{order.TotalAmount.Amount}|{order.Status}|{order.UserId}";
        using var sha256 = SHA256.Create();
        return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }

    public bool VerifyChecksum(Order order, string checksum)
    {
        var expectedChecksum = GenerateChecksum(order);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedChecksum),
            Encoding.UTF8.GetBytes(checksum));
    }
}
```

---

## 9. A09:2021 - SECURITY LOGGING AND MONITORING

### 9.1 Security Event Logging

```csharp
// ✅ DOĞRU: Log security events
public class SecurityEventLogger : ISecurityEventLogger
{
    private readonly ILogger<SecurityEventLogger> _logger;

    public void LogLoginSuccess(Guid userId, string ipAddress)
    {
        _logger.LogInformation(
            "LOGIN_SUCCESS | UserId: {UserId} | IP: {IpAddress} | Time: {Time}",
            userId, ipAddress, DateTime.UtcNow);
    }

    public void LogLoginFailure(string email, string ipAddress, string reason)
    {
        _logger.LogWarning(
            "LOGIN_FAILURE | Email: {EmailHash} | IP: {IpAddress} | Reason: {Reason} | Time: {Time}",
            HashEmail(email), ipAddress, reason, DateTime.UtcNow);
    }

    public void LogAccessDenied(Guid userId, string resource, string action)
    {
        _logger.LogWarning(
            "ACCESS_DENIED | UserId: {UserId} | Resource: {Resource} | Action: {Action} | Time: {Time}",
            userId, resource, action, DateTime.UtcNow);
    }

    public void LogSuspiciousActivity(Guid userId, string activity, string details)
    {
        _logger.LogWarning(
            "SUSPICIOUS_ACTIVITY | UserId: {UserId} | Activity: {Activity} | Details: {Details} | Time: {Time}",
            userId, activity, details, DateTime.UtcNow);
    }

    public void LogDataAccess(Guid userId, string entity, Guid entityId, string action)
    {
        _logger.LogInformation(
            "DATA_ACCESS | UserId: {UserId} | Entity: {Entity} | EntityId: {EntityId} | Action: {Action}",
            userId, entity, entityId, action);
    }

    private static string HashEmail(string email)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(email.ToLowerInvariant()));
        return Convert.ToHexString(hash)[..16]; // First 16 chars
    }
}
```

### 9.2 Sensitive Data Protection in Logs

```csharp
// ❌ YASAK: Sensitive data loglama
logger.LogInformation("User: {Email}", user.Email);              // PII!
logger.LogDebug("Token: {Token}", accessToken);                   // SECRET!
logger.LogInformation("Card: {CardNumber}", payment.CardNumber);  // PCI!
logger.LogInformation("Password: {Password}", password);          // SECRET!
logger.LogInformation("SSN: {SSN}", customer.SocialSecurityNumber); // PII!
logger.LogInformation("Phone: {Phone}", user.PhoneNumber);        // PII!
logger.LogInformation("Address: {Address}", user.Address);        // PII!

// ✅ DOĞRU: Sadece ID ve non-sensitive data
logger.LogInformation("User logged in: {UserId}", user.Id);
logger.LogInformation("Token generated for user: {UserId}", userId);
logger.LogInformation("Payment processed: {PaymentId}", payment.Id);
logger.LogInformation("Order created: {OrderId}, Amount: {Amount}", order.Id, order.TotalAmount);
```

---

## 10. A10:2021 - SERVER-SIDE REQUEST FORGERY (SSRF)

### 10.1 URL Validation

```csharp
// ✅ DOĞRU: Validate URLs
public class UrlValidator : IUrlValidator
{
    private readonly string[] _allowedHosts = { "api.stripe.com", "api.paypal.com", "cdn.merge.com" };
    private readonly string[] _blockedIpRanges = { "10.", "172.16.", "192.168.", "127.", "169.254." };

    public bool IsAllowed(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        // Only HTTPS
        if (uri.Scheme != "https")
            return false;

        // Check whitelist
        if (!_allowedHosts.Contains(uri.Host.ToLowerInvariant()))
            return false;

        // Block private IPs
        if (IPAddress.TryParse(uri.Host, out var ip))
        {
            var ipString = ip.ToString();
            if (_blockedIpRanges.Any(range => ipString.StartsWith(range)))
                return false;
        }

        // Block localhost
        if (uri.Host is "localhost" or "127.0.0.1" or "::1")
            return false;

        return true;
    }
}

// ✅ DOĞRU: Safe HTTP client
public class SafeHttpClient : ISafeHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IUrlValidator _urlValidator;

    public async Task<string> FetchAsync(string url, CancellationToken ct)
    {
        if (!_urlValidator.IsAllowed(url))
            throw new SecurityException($"URL not allowed: {url}");

        return await _httpClient.GetStringAsync(url, ct);
    }
}
```

---

## 11. INPUT VALIDATION (FluentValidation)

### 11.1 Command Validation

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(IRepository<Category> categoryRepository)
    {
        // String validation
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters")
            .Must(NotContainHtml).WithMessage("Name cannot contain HTML");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters")
            .Must(NotContainScript).WithMessage("Description cannot contain scripts");

        // SKU format
        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .Matches(@"^[A-Z0-9-]{3,50}$").WithMessage("Invalid SKU format");

        // Numeric validation
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be positive")
            .LessThanOrEqualTo(1_000_000).WithMessage("Price cannot exceed 1,000,000");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative")
            .LessThanOrEqualTo(100_000).WithMessage("Stock cannot exceed 100,000");

        // Foreign key validation
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required")
            .MustAsync(async (id, ct) => await categoryRepository.ExistsAsync(id, ct))
            .WithMessage("Category not found");
    }

    private static bool NotContainHtml(string value)
    {
        if (string.IsNullOrEmpty(value))
            return true;
        return !Regex.IsMatch(value, @"<[^>]+>");
    }

    private static bool NotContainScript(string value)
    {
        if (string.IsNullOrEmpty(value))
            return true;
        var lower = value.ToLowerInvariant();
        return !lower.Contains("<script") && !lower.Contains("javascript:");
    }
}
```

### 11.2 File Upload Validation

```csharp
public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly string[] _allowedContentTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public UploadImageCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required")
            .Must(f => f.Length > 0).WithMessage("File is empty")
            .Must(f => f.Length <= MaxFileSizeBytes).WithMessage($"File size cannot exceed {MaxFileSizeBytes / 1024 / 1024} MB")
            .Must(HaveAllowedExtension).WithMessage("File type not allowed")
            .Must(HaveAllowedContentType).WithMessage("File content type not allowed")
            .MustAsync(BeValidImage).WithMessage("File is not a valid image");
    }

    private bool HaveAllowedExtension(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }

    private bool HaveAllowedContentType(IFormFile file)
    {
        return _allowedContentTypes.Contains(file.ContentType.ToLowerInvariant());
    }

    private async Task<bool> BeValidImage(IFormFile file, CancellationToken ct)
    {
        try
        {
            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream, ct);
            return image.Width > 0 && image.Height > 0;
        }
        catch
        {
            return false;
        }
    }
}
```

---

## 12. FILES TO NEVER COMMIT

### .gitignore

```gitignore
# Secrets
.env
.env.*
*.env
appsettings.Development.json
appsettings.Local.json
appsettings.*.local.json
secrets.json
*.pfx
*.key
*.pem
*.p12
*.cer

# Credentials
credentials.json
service-account.json
google-credentials.json
aws-credentials
.aws/

# IDE
.idea/
.vs/
*.user
*.suo

# Build
bin/
obj/
*.dll

# Logs (may contain sensitive data)
logs/
*.log
```

---

## SECURITY CHECKLIST

Her PR'da kontrol et:

### Access Control
- [ ] Tüm endpoint'lerde [Authorize] var
- [ ] IDOR protection implemented
- [ ] Role/Policy-based authorization kullanılmış
- [ ] Resource ownership verified

### Cryptography
- [ ] Passwords BCrypt ile hash'lenmiş
- [ ] Tokens SHA256 ile hash'lenmiş
- [ ] No weak algorithms (MD5, SHA1)
- [ ] Secrets environment variable'da

### Injection
- [ ] Parameterized queries kullanılmış
- [ ] No string concatenation in SQL
- [ ] Input validation implemented

### Authentication
- [ ] JWT token expiration set (15-30 min)
- [ ] Refresh token rotation implemented
- [ ] Account lockout enabled
- [ ] Password requirements enforced

### Logging
- [ ] No PII in logs
- [ ] No secrets in logs
- [ ] Security events logged
- [ ] Structured logging used

### Configuration
- [ ] Security headers added
- [ ] CORS properly configured
- [ ] HTTPS enforced
- [ ] Rate limiting enabled

### Validation
- [ ] FluentValidation on all commands
- [ ] File upload validation
- [ ] URL validation for external requests
