using Merge.Application;
using Merge.Application.Configuration;
using Merge.Application.Interfaces;
using Merge.Application.Services;
using Merge.Infrastructure;
using Merge.Domain.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Modules.Identity;
using UserEntity = Merge.Domain.Modules.Identity.User;
using RoleEntity = Merge.Domain.Modules.Identity.Role;
using Merge.Domain.SharedKernel;
using Merge.API.Middleware;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.Http.Resilience;
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

// OpenTelemetry paket versiyonları .NET 9.0 ile uyumlu hale getirildi
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation());

builder.Services.Configure<OrderSettings>(
    builder.Configuration.GetSection(OrderSettings.SectionName));
builder.Services.Configure<PaymentSettings>(
    builder.Configuration.GetSection(PaymentSettings.SectionName));
builder.Services.Configure<LoyaltySettings>(
    builder.Configuration.GetSection(LoyaltySettings.SectionName));
builder.Services.Configure<ReferralSettings>(
    builder.Configuration.GetSection(ReferralSettings.SectionName));
builder.Services.Configure<MarketingSettings>(
    builder.Configuration.GetSection(MarketingSettings.SectionName));
builder.Services.Configure<B2BSettings>(
    builder.Configuration.GetSection(B2BSettings.SectionName));
builder.Services.Configure<AnalyticsSettings>(
    builder.Configuration.GetSection(AnalyticsSettings.SectionName));
builder.Services.Configure<CartSettings>(
    builder.Configuration.GetSection(CartSettings.SectionName));
builder.Services.Configure<ContentSettings>(
    builder.Configuration.GetSection(ContentSettings.SectionName));
builder.Services.Configure<PaginationSettings>(
    builder.Configuration.GetSection(PaginationSettings.SectionName));
builder.Services.Configure<ReviewSettings>(
    builder.Configuration.GetSection(ReviewSettings.SectionName));
builder.Services.Configure<SellerSettings>(
    builder.Configuration.GetSection(SellerSettings.SectionName));
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.Configure<SecuritySettings>(
    builder.Configuration.GetSection(SecuritySettings.SectionName));
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<TwoFactorAuthSettings>(
    builder.Configuration.GetSection(TwoFactorAuthSettings.SectionName));
builder.Services.Configure<SupportSettings>(
    builder.Configuration.GetSection(SupportSettings.SectionName));
builder.Services.Configure<MLSettings>(
    builder.Configuration.GetSection(MLSettings.SectionName));
builder.Services.Configure<ServiceSettings>(
    builder.Configuration.GetSection(ServiceSettings.SectionName));
builder.Services.Configure<SearchSettings>(
    builder.Configuration.GetSection(SearchSettings.SectionName));
builder.Services.Configure<UserSettings>(
    builder.Configuration.GetSection(UserSettings.SectionName));
builder.Services.Configure<InternationalSettings>(
    builder.Configuration.GetSection(InternationalSettings.SectionName));
builder.Services.Configure<ShippingSettings>(
    builder.Configuration.GetSection(ShippingSettings.SectionName));
builder.Services.Configure<CacheSettings>(
    builder.Configuration.GetSection(CacheSettings.SectionName));
builder.Services.Configure<RecommendationSettings>(
    builder.Configuration.GetSection(RecommendationSettings.SectionName));

// Add services to the container
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
    // Bu, Swagger endpoint'lerinin /swagger/v1/swagger.json formatında olmasını sağlar
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration
builder.Services.AddSwaggerGen(options =>
{
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
    
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    
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

// External service call'lar için retry, circuit breaker ve timeout policy'leri ekleniyor
// Note: AddStandardResilienceHandler extension method requires IHttpClientBuilder
// The method chain AddHttpClient().AddStandardResilienceHandler() should work
// If it doesn't, we may need to configure resilience per named/typed client
builder.Services.AddHttpClient();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "Merge:";
});

builder.Services.AddScoped<ICacheService, CacheService>();

// Identity configuration
builder.Services.AddIdentity<UserEntity, RoleEntity>(options =>
{
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
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key bulunamadı. JWT_SECRET_KEY environment variable veya appsettings Jwt:Key tanımlayın.");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer bulunamadı");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience bulunamadı");

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

// Note: For API-only applications, CSRF protection is typically handled via token validation
// For web applications with forms, use AddAntiforgery() and ValidateAntiForgeryToken attribute
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__Host-CSRF";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.SameAsRequest 
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

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
builder.Services.AddCors(options =>
{
    // Development için gevşek policy
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

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

app.UseResponseCompression();

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

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("Production");
}

app.UseStaticFiles(); // wwwroot için

app.UseSession();

// Security middlewares
app.UseRateLimiting();
app.UseIpWhitelist();

app.UseMiddleware<Merge.API.Middleware.IdempotencyKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

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
            // This runs pending migrations and maintains migration history
            dbContext.Database.Migrate();

            logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database migration failed. This is expected if PostgreSQL is not running. Error: {ErrorMessage}", ex.Message);
            logger.LogWarning("To start PostgreSQL, run: docker-compose up -d");
            logger.LogWarning("Or start PostgreSQL manually and ensure connection string is correct in appsettings.json");
            // Production'da migration'lar CI/CD pipeline'da çalıştırılmalı (BOLUM 6.0)
        }
    }
}

// Note: For production, run migrations via CI/CD pipeline:
// dotnet ef database update --project Merge.Infrastructure --startup-project Merge.API

app.Run();
