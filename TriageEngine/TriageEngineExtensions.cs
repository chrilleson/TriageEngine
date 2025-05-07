using Microsoft.Extensions.DependencyInjection;
using TriageEngine.Actions.Factory;
using TriageEngine.Services;

namespace TriageEngine;

public static class TriageEngineExtensions
{
    public static IServiceCollection AddTriageEngine(this IServiceCollection services)
    {
        services.AddScoped<ITriageService, TriageService>();
        services.AddScoped<ITriageEngine, TriageEngine>();
        services.AddSingleton<IActionFactory, ActionFactory>();

        return services;
    }
}