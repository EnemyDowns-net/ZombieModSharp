using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using ZombieModSharp.Abstractions;
using ZombieModSharp.Core.Services;

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
    private readonly IPlayerManager _player;
    private readonly IInfect _infect;
    private readonly IListeners _listeners;
    private readonly IZTele _ztele;
    private readonly ICommand _command;
    private readonly IHooks _hooks;
    private readonly IKnockback _knockback;
    private readonly IWeapons _weapons;
    private readonly IConfigs _configs;

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
        
        // Register external dependencies
        services.AddSingleton(_sharedSystem);
        services.AddSingleton(_sharedSystem.GetEntityManager());
        services.AddSingleton(_sharedSystem.GetEventManager());
        services.AddSingleton(_sharedSystem.GetModSharp());
        services.AddSingleton(_sharedSystem.GetClientManager());
        
        // Register our services using the extension method
        services.AddZombieModSharpServices();

        // _bridge = new InterfaceBridge(dllPath, sharpPath, version, sharedSystem);
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<ZombieModSharp>();
        _serviceProvider = services.BuildServiceProvider();

        // Get services from DI container instead of manual instantiation
        _player = _serviceProvider.GetRequiredService<IPlayerManager>();
        _infect = _serviceProvider.GetRequiredService<IInfect>();
        _ztele = _serviceProvider.GetRequiredService<IZTele>();
        _eventListener = _serviceProvider.GetRequiredService<IEvents>();
        _listeners = _serviceProvider.GetRequiredService<IListeners>();
        _command = _serviceProvider.GetRequiredService<ICommand>();
        _hooks = _serviceProvider.GetRequiredService<IHooks>();
        _knockback = _serviceProvider.GetRequiredService<IKnockback>();
        _weapons = _serviceProvider.GetRequiredService<IWeapons>();
        _configs = _serviceProvider.GetRequiredService<IConfigs>();
    }

    public bool Init()
    {
        _logger.LogInformation(
            "Oh wow, we seem to be crossing paths a lot lately... Where could I have seen you before? Can you figure it out?");

        _listeners.Init();
        _eventListener.Init();

        var _gamedata = _sharedSystem.GetModSharp().GetGameData();
        _gamedata.Register("ZombieModSharp.jsonc");

        return true;

    }

    public void Shutdown()
    {
        // _logger.LogInformation("See you around, Nameless~ Try to stay out of trouble, especially... the next time we meet!");
    }

    public void PostInit()
    {
        // _logger.LogInformation("Why don't you stay and play for a while?");
        _eventListener.RegisterEvents();
        _hooks.PostInit();
        _command.PostInit();
        _configs.PostInit();
    }

    public void OnAllModulesLoaded()
    {

    }

    public void OnLibraryConnected(string name)
    {
        //_logger.LogInformation("The~ Game~ Is~ On~");
    }

    public void OnLibraryDisconnect(string name)
    {
        
    }
}