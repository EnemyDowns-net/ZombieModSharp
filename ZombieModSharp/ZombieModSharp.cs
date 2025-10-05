using System.Diagnostics.Tracing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using ZombieModSharp.Core;
using ZombieModSharp.Core.Infection;
using ZombieModSharp.Core.Player;
using ZombieModSharp.Interface.Events;
using ZombieModSharp.Interface.Infection;
using ZombieModSharp.Interface.Player;

namespace ZombieModSharp;

public sealed class ZombieModSharp : IModSharpModule
{
    public string DisplayName   => "Zombie ModSharp";
    public string DisplayAuthor => "Oylsister";

    private readonly ILogger<ZombieModSharp> _logger;
    private readonly InterfaceBridge  _bridge;
    private readonly ServiceProvider  _serviceProvider;
    private readonly ISharedSystem _sharedSystem;
    private readonly IEvents _eventListener;
    private readonly IPlayerManager _playerManager;
    private readonly IInfect _infect;

    public ZombieModSharp(ISharedSystem sharedSystem,
                      string dllPath,
                      string sharpPath,
                      Version? version,
                      IConfiguration coreConfiguration,
                      bool hotReload)
    {
        ArgumentNullException.ThrowIfNull(dllPath);
        ArgumentNullException.ThrowIfNull(sharpPath);
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(coreConfiguration);

        _sharedSystem = sharedSystem ?? throw new ArgumentNullException(nameof(sharedSystem));
        var configuration = new ConfigurationBuilder()
                            .AddJsonFile(Path.Combine(dllPath, "appsettings.json"), false, false)
                            .Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(sharedSystem.GetLoggerFactory());
        services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));

        _bridge = new InterfaceBridge(dllPath, sharpPath, version, sharedSystem);
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<ZombieModSharp>();
        _serviceProvider = services.BuildServiceProvider();

        // Initial our stuff.
        _playerManager = new PlayerManager();
        _infect = new Infect(_bridge.EntityManager, _bridge.EventManager, _serviceProvider.GetRequiredService<ILogger<Infect>>(), _playerManager, _bridge.GameRules);
        _eventListener = new Events(_bridge.EventManager, _serviceProvider.GetRequiredService<ILogger<Events>>(), _bridge.ClientManager, _playerManager, _bridge.EntityManager, _infect);
    }

    public bool Init()
    {
        _logger.LogInformation(
            "Oh wow, we seem to be crossing paths a lot lately... Where could I have seen you before? Can you figure it out?");
        _bridge.ModSharp.GetGameRules();
        return true;
    }

    public void Shutdown()
    {
        _logger.LogInformation("See you around, Nameless~ Try to stay out of trouble, especially... the next time we meet!");
    }

    public void PostInit()
    {
        _logger.LogInformation("Why don't you stay and play for a while?");

        var eventManager = _sharedSystem.GetEventManager();
        eventManager.InstallEventListener((IEventListener)_eventListener);

        _eventListener.RegisterEvents();
    }

    public void OnAllModulesLoaded()
    {
        _logger.LogInformation("A foolish sage or a wise fool... Who will I become next?");
    }

    public void OnLibraryConnected(string name)
    {
        _logger.LogInformation("The~ Game~ Is~ On~");
    }

    public void OnLibraryDisconnect(string name)
    {
        _logger.LogInformation("Done playing for today...");
    }
}