using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace LiftTracker.Infrastructure.Logging;

public static class LoggingExtensions
{
    public static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        Log.Logger = CreateLogger(configuration, environment);

        services.AddSerilog();

        return services;
    }

    private static ILogger CreateLogger(IConfiguration configuration, IHostEnvironment environment)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environment", environment.EnvironmentName)
            .Enrich.WithProperty("Application", "LiftTracker");

        // Console logging for all environments
        loggerConfig.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        // File logging configuration
        var logsPath = configuration.GetValue<string>("Logging:FilePath") ?? "logs/lift-tracker-.log";

        loggerConfig.WriteTo.File(
            path: logsPath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            fileSizeLimitBytes: 10_000_000, // 10MB
            rollOnFileSizeLimit: true,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        // Set minimum log level based on environment
        var minimumLevel = environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information;
        loggerConfig.MinimumLevel.Is(minimumLevel);

        // Override levels for Microsoft and System namespaces
        loggerConfig.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
        loggerConfig.MinimumLevel.Override("System", LogEventLevel.Warning);
        loggerConfig.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information);

        return loggerConfig.CreateLogger();
    }
}
