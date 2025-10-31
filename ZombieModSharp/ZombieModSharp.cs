using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PlayerManager_Shared.Abstractions;
using Sharp.Shared;
using Sharp.Shared.Definition;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using ZombieModSharp.Core;
using ZombieModSharp.Core.Infection;
using ZombieModSharp.Core.Player;
using ZombieModSharp.Interface.Command;
using ZombieModSharp.Interface.Events;
using ZombieModSharp.Interface.Infection;
using ZombieModSharp.Interface.Listeners;
using ZombieModSharp.Interface.Player;
using ZombieModSharp.Interface.ZTele;

namespace ZombieModSharp;

public sealed class ZombieModSharp : IModSharpModule
{
    public string DisplayName   => "Zombie ModSharp";
    public string DisplayAuthor => "Oylsister";

    private readonly ILogger<ZombieModSharp> _logger;
    // private readonly InterfaceBridge  _bridge;
    private readonly ServiceProvider  _serviceProvider;
    private readonly ISharedSystem _sharedSystem;
    private readonly IEvents _eventListener;
    private readonly IPlayer _player;
    private readonly IInfect _infect;
    private readonly IListeners _listeners;
    private readonly IZTele _ztele;
    private readonly ICommand _command;

    // outside module

    public ZombieModSharp(ISharedSystem sharedSystem,
                      string dllPath,
                      string sharpPath,
                      Version? version,
                      IConfiguration? coreConfiguration,
                      bool hotReload)
    {
        ArgumentNullException.ThrowIfNull(dllPath);
        ArgumentNullException.ThrowIfNull(sharpPath);
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(coreConfiguration);

        _sharedSystem = sharedSystem ?? throw new ArgumentNullException(nameof(sharedSystem));
        //var configuration = new ConfigurationBuilder().AddJsonFile(Path.Combine(dllPath, "appsettings.json"), false, false).Build();

        var services = new ServiceCollection();

        services.AddSingleton(sharedSystem.GetLoggerFactory());
        services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));

        // _bridge = new InterfaceBridge(dllPath, sharpPath, version, sharedSystem);
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<ZombieModSharp>();
        _serviceProvider = services.BuildServiceProvider();

        // Initial our stuff.
        _player = new Player();
        _infect = new Infect(_sharedSystem.GetEntityManager(), _sharedSystem.GetEventManager(), _serviceProvider.GetRequiredService<ILogger<Infect>>(), _player, _sharedSystem.GetModSharp());
        _ztele = new ZTele(_player, _serviceProvider.GetRequiredService<ILogger<ZTele>>(), _sharedSystem.GetEntityManager());
        _eventListener = new Events(_sharedSystem, _serviceProvider.GetRequiredService<ILogger<Events>>(), _player, _infect, _ztele);
        _listeners = new Listeners(_player, _sharedSystem, _serviceProvider.GetRequiredService<ILogger<Listeners>>());
        _command = new Command(_player, _ztele, _infect, _sharedSystem);
    }

    public bool Init()
    {
        _logger.LogInformation(
            "Oh wow, we seem to be crossing paths a lot lately... Where could I have seen you before? Can you figure it out?");

        _listeners.Init();
        _eventListener.Init();
        return true;
    }

    public void Shutdown()
    {
        _logger.LogInformation("See you around, Nameless~ Try to stay out of trouble, especially... the next time we meet!");
    }

    public void PostInit()
    {
        _logger.LogInformation("Why don't you stay and play for a while?");
        _eventListener.RegisterEvents();
    }

    public void OnAllModulesLoaded()
    {
        var wrapper = _sharedSystem.GetSharpModuleManager()
                .GetRequiredSharpModuleInterface<IPlayerManager>(IPlayerManager.Identity);
    }

    public void OnLibraryConnected(string name)
    {
        _logger.LogInformation("The~ Game~ Is~ On~");
    }

    public void OnLibraryDisconnect(string name)
    {
        
    }

    /*
    private bool HasPermission(IGameClient client)
    {
        if (_permission is null) return false;

        var steamId = client.SteamId.ToString();
        var identity = _permission.GetIdentity(steamId);

        return identity.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || identity.Equals("Manager", StringComparison.OrdinalIgnoreCase)
                || identity.Equals("Owner", StringComparison.OrdinalIgnoreCase);
    }
    */
}