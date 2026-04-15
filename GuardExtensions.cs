using Microsoft.Extensions.DependencyInjection;
using ControlAgentNet.Core.Abstractions;

namespace ControlAgentNet.Guards;

public static class GuardExtensions
{
    public static IServiceCollection AddToolGuard<TGuard>(this IServiceCollection services)
        where TGuard : class, IToolGuard
    {
        services.AddSingleton<IToolGuard, TGuard>();
        return services;
    }

    public static IServiceCollection AddRiskDenyGuard(this IServiceCollection services, Action<RiskDenyGuardOptions>? configure = null)
    {
        var options = new RiskDenyGuardOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton<IToolGuard, RiskDenyGuard>();
        return services;
    }

    public static IServiceCollection AddToolAllowlistGuard(this IServiceCollection services, Action<ToolAllowlistGuardOptions>? configure = null)
    {
        var options = new ToolAllowlistGuardOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton<IToolGuard, ToolAllowlistGuard>();
        return services;
    }
}
