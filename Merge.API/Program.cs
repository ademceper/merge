using Merge.Application;
using Merge.Infrastructure;
using Merge.Domain.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;
using Merge.API.Middleware;
using Merge.Application.Interfaces;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Data.Contexts;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ✅ BOLUM 5.0: OpenTelemetry tracing + metrics (ZORUNLU)
// TODO: OpenTelemetry paket versiyonları uyumlu değil - Production'da güncellenmeli
// Şimdilik yorum satırına alındı - Paket versiyonları düzeltildikten sonra aktif edilecek
// Production'da tam OpenTelemetry setup için:
// - OpenTelemetry.Instrumentation.AspNetCore versiyonları güncellenmeli
// - OpenTelemetry.Exporter.Prometheus.AspNetCore versiyonları güncellenmeli
// - Jaeger exporter eklenebilir
/*
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());
*/

// ✅ CONFIGURATION: Business settings (BEST_PRACTICES_ANALIZI.md - BOLUM 2.1.4)
builder.Services.Configure<Merge.Application.Configuration.OrderSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.OrderSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.PaymentSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.PaymentSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.LoyaltySettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.LoyaltySettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.ReferralSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.ReferralSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.MarketingSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.MarketingSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.B2BSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.B2BSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.AnalyticsSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.AnalyticsSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.CartSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.CartSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.ContentSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.ContentSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.PaginationSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.PaginationSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.ReviewSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.ReviewSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.SellerSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.SellerSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.EmailSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.EmailSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.SecuritySettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.SecuritySettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.JwtSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.JwtSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.TwoFactorAuthSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.TwoFactorAuthSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.SupportSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.SupportSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.MLSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.MLSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.ServiceSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.ServiceSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.SearchSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.SearchSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.UserSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.UserSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.InternationalSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.InternationalSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.ShippingSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.ShippingSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.CacheSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.CacheSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.RecommendationSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.RecommendationSettings.SectionName));

// Add services to the container
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
// ✅ BOLUM 4.1.5: Content Negotiation - JSON, XML, CSV format desteği (ZORUNLU)
builder.Services.AddControllers()
    .AddXmlSerializerFormatters()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // ✅ BOLUM 4.1.4: RFC 7807 Problem Details (ZORUNLU)
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Type = "https://api.merge.com/errors/validation-error",
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.HttpContext.Request.Path,
                Detail = "One or more validation errors occurred.",
                Extensions =
                {
                    ["errors"] = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()),
                    ["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier,
                    ["timestamp"] = DateTimeOffset.UtcNow
                }
            };
            
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(problemDetails);
        };
    });
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    // ✅ GroupNameFormat "'v'V" - sadece major version (v1, v2, vb.)
    // Bu, Swagger endpoint'lerinin /swagger/v1/swagger.json formatında olmasını sağlar
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
builder.Services.AddSwaggerGen(options =>
{
    // ✅ API Versioning ile uyumlu Swagger yapılandırması
    // IApiVersionDescriptionProvider kullanarak her versiyon için dinamik SwaggerDoc oluştur
    // Bu, "No operations defined in spec!" hatasını çözer
    // GroupNameFormat "'v'V" olduğu için "v1" formatı kullanılır
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Merge E-Commerce API",
        Version = "v1.0",
        Description = "e-ticaret backend API v1.0"
    });
    
    // API versioning için Swagger yapılandırması
    options.AddServer(new OpenApiServer { Url = "https://api.mergecommerce.com" });

    // ✅ XML documentation için (BOLUM 4.1)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // JWT Authentication için Swagger yapılandırması
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // ✅ API Versioning için: ResolveConflictingActions kullanarak çakışmaları çöz
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    
    // ✅ API Versioning için: VersionedApiExplorer ile entegrasyon
    // Bu, Swagger'ın versioned API'leri bulmasını sağlar
    // GroupNameFormat "'v'V" olduğu için "v1" formatı kullanılır
    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        // VersionedApiExplorer kullanıldığında, docName GroupNameFormat'e göre oluşturulur
        // GroupNameFormat "'v'V" olduğu için "v1" formatı kullanılır
        // apiDesc.GroupName ile docName'i karşılaştır
        return apiDesc.GroupName == docName;
    });
});

// ✅ BOLUM 1.1: Clean Architecture - Dependency Injection (ZORUNLU)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ✅ BOLUM 10.2: Redis distributed cache (ZORUNLU)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "Merge:";
});

// ✅ BOLUM 10.2: Cache service registration
builder.Services.AddScoped<Merge.Application.Interfaces.ICacheService, Merge.Application.Services.CacheService>();

// Identity configuration
builder.Services.AddIdentity<Merge.Domain.Modules.Identity.User, Merge.Domain.Modules.Identity.Role>(options =>
{
    // ✅ SECURITY: Güçlü password policy (BOLUM 5.3)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.Password.RequiredUniqueChars = 4;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; 
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// JWT Authentication
// ✅ SECURITY: JWT Secret önce environment variable'dan al, yoksa appsettings'ten
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key bulunamadı. JWT_SECRET_KEY environment variable veya appsettings Jwt:Key tanımlayın.");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer bulunamadı");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience bulunamadı");

// ✅ SECURITY: Production'da hardcoded key kullanımını engelle
if (!builder.Environment.IsDevelopment() && jwtKey == "YourSuperSecretKeyThatIsAtLeast32CharactersLong!")
{
    throw new InvalidOperationException("CRITICAL SECURITY ERROR: Production'da varsayılan JWT key kullanılamaz! JWT_SECRET_KEY environment variable tanımlayın.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ✅ SECURITY: Rate Limiting (BOLUM 3.3)
// Note: Rate limiting is handled by custom RateLimitingMiddleware
// Configuration is done via RateLimitAttribute on controllers

// Session for rate limiting (optional, if not using distributed cache)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// CORS
// ✅ SECURITY: Production için güvenli CORS yapılandırması
builder.Services.AddCors(options =>
{
    // Development için gevşek policy
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    // ✅ SECURITY: Production için güvenli CORS policy
    options.AddPolicy("Production", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "https://mergecommerce.com", "https://www.mergecommerce.com" };

        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "X-CSRF-TOKEN")
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    
    // ✅ API Versioning ile uyumlu Swagger UI yapılandırması
    // IApiVersionDescriptionProvider kullanarak her versiyon için dinamik endpoint oluştur
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    
    app.UseSwaggerUI(c =>
    {
        // Her versiyon için dinamik olarak Swagger endpoint oluştur
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Merge E-Commerce API {description.GroupName.ToUpperInvariant()}");
        }
        
        c.RoutePrefix = "swagger"; // Swagger UI: /swagger
    });
}

// Global Exception Handler - En üstte olmalı
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

// ✅ BOLUM 7.1: Response Compression (ZORUNLU)
app.UseResponseCompression();

// ✅ SECURITY: Security Headers (BOLUM 5.2)
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    }
    await next();
});

// ✅ SECURITY: Development'ta AllowAll, Production'da güvenli CORS policy kullan
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("Production");
}

app.UseStaticFiles(); // wwwroot için

// Security middlewares
app.UseRateLimiting();
app.UseIpWhitelist();

app.UseAuthentication();
app.UseAuthorization();

// ✅ BOLUM 5.0: Health Checks (ZORUNLU - Gerçek health check)
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapControllers();

// ✅ CRITICAL FIX: Database migration strategy
// BEFORE: EnsureCreated() bypasses migrations (causes data loss, no rollback)
// AFTER: Migrate() runs pending migrations properly
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            // ✅ Use Migrate() instead of EnsureCreated()
            // This runs pending migrations and maintains migration history
            dbContext.Database.Migrate();

            logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database migration failed. This is expected if PostgreSQL is not running. Error: {ErrorMessage}", ex.Message);
            logger.LogWarning("To start PostgreSQL, run: docker-compose up -d");
            logger.LogWarning("Or start PostgreSQL manually and ensure connection string is correct in appsettings.json");
            // ✅ Development'ta migration hatası uygulamayı crash etmesin
            // Production'da migration'lar CI/CD pipeline'da çalıştırılmalı (BOLUM 6.0)
        }
    }
}

// Note: For production, run migrations via CI/CD pipeline:
// dotnet ef database update --project Merge.Infrastructure --startup-project Merge.API

app.Run();
