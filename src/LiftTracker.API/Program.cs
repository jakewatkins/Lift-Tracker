using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;
using System.Text.Json;
using Serilog;
using Serilog.Events;
using LiftTracker.Infrastructure.Data;
using LiftTracker.Domain.Interfaces;
using LiftTracker.Infrastructure.Repositories;
using LiftTracker.Infrastructure.Authentication;
using LiftTracker.Infrastructure.Logging;
using LiftTracker.Application.Interfaces;
using LiftTracker.Application.Services;
using LiftTracker.API.Middleware;
using LiftTracker.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Enhanced configuration setup
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables("LIFTTRACKER_")
    .AddCommandLine(args);

// Configure strongly-typed configuration options
builder.Services.Configure<ApplicationOptions>(
    builder.Configuration.GetSection(ApplicationOptions.SectionName));
builder.Services.Configure<GoogleAuthOptions>(
    builder.Configuration.GetSection(GoogleAuthOptions.SectionName));

// Configure Serilog
builder.Services.AddLogging(builder.Configuration, builder.Environment);
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddDbContext<LiftTrackerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        var clientUrl = builder.Configuration["ClientApp:BaseUrl"] ?? "https://localhost:5001";

        policy.WithOrigins(
                clientUrl,
                "https://localhost:5001",
                "http://localhost:5000",
                "https://localhost:7001", // Common Blazor WASM dev ports
                "http://localhost:7000"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowedToAllowWildcardSubdomains();
    });

    // Development-only policy for broader access
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("Development", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
});

// Add controllers
builder.Services.AddControllers();

// Configure HTTPS redirection
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
    options.HttpsPort = builder.Environment.IsDevelopment() ? 7001 : 443;
});

// Configure HSTS (HTTP Strict Transport Security)
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
    options.ExcludedHosts.Clear(); // Remove default localhost exclusions in production
});

// Register authentication services
builder.Services.AddHttpClient<GoogleAuthService>();
builder.Services.AddScoped<JwtTokenService>();

// Configure authentication with Google OAuth and JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["GoogleAuth:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["GoogleAuth:ClientSecret"] ?? "";
    options.CallbackPath = "/api/auth/callback";
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.SaveTokens = true;
})
.AddJwtBearer(options =>
{
    var googleAuthOptions = builder.Configuration.GetSection(GoogleAuthOptions.SectionName).Get<GoogleAuthOptions>();
    if (googleAuthOptions?.JwtKey != null)
    {
        var key = Encoding.UTF8.GetBytes(googleAuthOptions.JwtKey);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = googleAuthOptions.JwtIssuer,
            ValidAudience = googleAuthOptions.JwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };

        // Handle JWT in query string for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    }
});

builder.Services.AddAuthorization();

// Configure caching services
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000; // Limit number of cached items
    options.CompactionPercentage = 0.25; // Remove 25% when size limit reached
});
builder.Services.AddSingleton<LiftTracker.Infrastructure.Caching.ICacheService, LiftTracker.Infrastructure.Caching.MemoryCacheService>();

// Configure response caching
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 64 * 1024; // 64KB max response size for caching
    options.UseCaseSensitivePaths = false;
});

// Register base repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IUserRepository>(provider =>
{
    var baseRepo = provider.GetRequiredService<UserRepository>();
    var cacheService = provider.GetRequiredService<LiftTracker.Infrastructure.Caching.ICacheService>();
    return new LiftTracker.Infrastructure.Repositories.CachedUserRepository(baseRepo, cacheService);
});
builder.Services.AddScoped<IWorkoutSessionRepository, WorkoutSessionRepository>();
builder.Services.AddScoped<IStrengthLiftRepository, StrengthLiftRepository>();
builder.Services.AddScoped<IMetconWorkoutRepository, MetconWorkoutRepository>();
builder.Services.AddScoped<IExerciseTypeRepository, ExerciseTypeRepository>();

// Register application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWorkoutSessionService, WorkoutSessionService>();
builder.Services.AddScoped<IStrengthLiftService, StrengthLiftService>();
builder.Services.AddScoped<IMetconWorkoutService, MetconWorkoutService>();
builder.Services.AddScoped<IProgressService, ProgressService>();

// Register enhanced cached services
builder.Services.AddScoped<LiftTracker.Application.Services.PerformanceOptimizedService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(LiftTracker.Application.Mappings.UserMappingProfile));

// Configure health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<LiftTrackerDbContext>("database", HealthStatus.Unhealthy, new[] { "database" })
    .AddCheck("api", () => HealthCheckResult.Healthy("API is running"), new[] { "api" })
    .AddCheck("memory", () =>
    {
        var allocatedBytes = GC.GetTotalMemory(false);
        var data = new Dictionary<string, object>
        {
            ["allocated"] = allocatedBytes,
            ["gen0"] = GC.CollectionCount(0),
            ["gen1"] = GC.CollectionCount(1),
            ["gen2"] = GC.CollectionCount(2)
        };
        var status = allocatedBytes < 1024L * 1024L * 1024L ? HealthStatus.Healthy : HealthStatus.Degraded;
        return HealthCheckResult.Healthy("Memory usage is normal", data);
    }, new[] { "memory" });

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LiftTracker API",
        Version = "v1.0.0",
        Description = "A comprehensive workout tracking API for strength training and metabolic conditioning workouts with JWT authentication, caching, and performance monitoring.",
        Contact = new OpenApiContact
        {
            Name = "LiftTracker Development Team",
            Email = "support@lifttracker.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License"
        }
    });

    // Configure JWT authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
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

    // Include XML comments for API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "LiftTracker API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "LiftTracker API Documentation";
    });
}

// Global error handling middleware (must be first)
app.UseErrorHandlingMiddleware();

// Add performance monitoring middleware (early in pipeline)
app.UseMiddleware<LiftTracker.API.Middleware.PerformanceMonitoringMiddleware>();

// Add security headers
app.UseSecurityHeaders();

// Add Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    // Customize request logging
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? LogEventLevel.Error
        : httpContext.Response.StatusCode > 499
            ? LogEventLevel.Error
            : LogEventLevel.Information;

    // Enrich with additional context
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);

        if (httpContext.User.Identity is { IsAuthenticated: true })
        {
            diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ??
                                             httpContext.User.FindFirst("id")?.Value);
        }
    };
});

// HSTS (HTTP Strict Transport Security) - only in production
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// CORS middleware
app.UseCors("AllowBlazorClient");

// Response caching middleware
app.UseResponseCaching();

// Authentication and authorization
app.UseAuthentication();
app.UseAuthenticationMiddleware();
app.UseAuthorization();

// Validation middleware
app.UseValidationMiddleware();

// Configure controllers
app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            environment = app.Environment.EnvironmentName,
            checks = report.Entries.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    status = kvp.Value.Status.ToString(),
                    description = kvp.Value.Description,
                    data = kvp.Value.Data,
                    duration = kvp.Value.Duration.TotalMilliseconds,
                    exception = kvp.Value.Exception?.Message
                })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
});

// Basic health check endpoint
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // Exclude all checks for liveness
});

// API info endpoint
app.MapGet("/api/info", () => new
{
    name = "LiftTracker API",
    version = "1.0.0",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow
}).WithTags("System").WithOpenApi();

app.Run();

// Ensure Serilog is properly disposed
Log.CloseAndFlush();

// Make the Program class accessible for testing
public partial class Program { }
