using Microsoft.Extensions.DependencyInjection;
using ZombieModSharp.Core;
using ZombieModSharp.Core.Infection;
using ZombieModSharp.Core.Player;
using ZombieModSharp.Interface.Events;
using ZombieModSharp.Interface.Infection;
using ZombieModSharp.Interface.Player;

namespace ZombieModSharp.services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZombieModSharpServices(this IServiceCollection services)
    {
        services.AddSingleton<IEvents, Events>()
            .AddSingleton<IPlayerManager, PlayerManager>()
            .AddSingleton<IInfect, Infect>();
        return services;
    }
}