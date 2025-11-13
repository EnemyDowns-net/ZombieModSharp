using Microsoft.Extensions.DependencyInjection;
using ZombieModSharp.Abstractions;
using ZombieModSharp.Core.HookManager;
using ZombieModSharp.Core.Modules;

namespace ZombieModSharp.Core.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZombieModSharpServices(this IServiceCollection services)
    {
        services.AddSingleton<IGameEventManager, GameEventManager>()
            .AddSingleton<IPlayerManager, PlayerManager>()
            .AddSingleton<IInfect, Infect>()
            .AddSingleton<IListeners, Listeners>()
            .AddSingleton<ICommand, Command>()
            .AddSingleton<IHooks, Hooks>()
            .AddSingleton<IKnockback, Knockback>()
            .AddSingleton<IConfigManager, ConfigManager>()
            .AddSingleton<ICvarManager, CvarManager>()
            .AddSingleton<IPlayerClasses, PlayerClasses>();
        return services;
    }
}