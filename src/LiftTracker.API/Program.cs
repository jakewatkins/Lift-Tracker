using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;
using LiftTracker.Infrastructure.Data;
using LiftTracker.Domain.Interfaces;
using LiftTracker.Infrastructure.Repositories;
using LiftTracker.Infrastructure.Authentication;
using LiftTracker.Infrastructure.Logging;
using LiftTracker.Application.Interfaces;
using LiftTracker.Application.Services;
using LiftTracker.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

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
        policy.WithOrigins(
                builder.Configuration["ClientApp:BaseUrl"] ?? "https://localhost:5001",
                "https://localhost:5001",
                "http://localhost:5000"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add controllers
builder.Services.AddControllers();

// Configure Google OAuth options
builder.Services.Configure<GoogleAuthOptions>(
    builder.Configuration.GetSection(GoogleAuthOptions.SectionName));

// Register authentication services
builder.Services.AddHttpClient<GoogleAuthService>();
builder.Services.AddScoped<JwtTokenService>();

// Configure JWT authentication
var googleAuthOptions = builder.Configuration.GetSection(GoogleAuthOptions.SectionName).Get<GoogleAuthOptions>();
if (googleAuthOptions?.JwtKey != null)
{
    var key = Encoding.UTF8.GetBytes(googleAuthOptions.JwtKey);
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
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
    });
}

builder.Services.AddAuthorization();

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
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

// Add AutoMapper (when mapping profile is created)
// builder.Services.AddAutoMapper(typeof(LiftTracker.Application.Mappings.MappingProfile));

// Configure health checks (will be added later with proper package references)
// builder.Services.AddHealthChecks();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LiftTracker API",
        Version = "v1",
        Description = "A comprehensive workout tracking API for strength training and metabolic conditioning workouts",
        Contact = new OpenApiContact
        {
            Name = "LiftTracker Team",
            Email = "support@lifttracker.com"
        }
    });

    // Configure JWT authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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

app.UseHttpsRedirection();

// CORS middleware
app.UseCors("AllowBlazorClient");

// Authentication and authorization
app.UseAuthentication();
app.UseAuthenticationMiddleware();
app.UseAuthorization();

// Validation middleware
app.UseValidationMiddleware();

// Configure controllers
app.MapControllers();

// Health check endpoint
// app.MapHealthChecks("/health");

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
