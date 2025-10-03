using Microsoft.Extensions.DependencyInjection;
using LiftTracker.Application.Interfaces;

namespace LiftTracker.Application;

/// <summary>
/// Extension methods for configuring Application layer services
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Adds Application layer services to the dependency injection container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add AutoMapper
        services.AddAutoMapper(typeof(ApplicationServiceExtensions));

        // Service registrations will be added here when services are implemented
        // services.AddScoped<IUserService, UserService>();
        // services.AddScoped<IWorkoutSessionService, WorkoutSessionService>();
        // services.AddScoped<IStrengthLiftService, StrengthLiftService>();
        // services.AddScoped<IMetconWorkoutService, MetconWorkoutService>();
        // services.AddScoped<IProgressService, ProgressService>();

        return services;
    }
}
