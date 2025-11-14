using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using ZombieModSharp.Abstractions;
using ZombieModSharp.Abstractions.Storage;
using ZombieModSharp.Core.Services;
using ZombieModSharp.Storage;

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
    private readonly IListeners _listeners;
    private readonly ICommand _command;
    private readonly IHooks _hooks;
    private readonly IConfigs _configs;
    private readonly ISqliteDatabase _sqliteDatabase;
    private readonly ICvarServices _cvarManager;

    public static string Prefix { get; } = " \x04[Z:MS]\x01";

    public ZombieModSharp(ISharedSystem sharedSystem,
                      string dllPath,
                      string sharpPath,
                      Version? version,
                      IConfiguration configuration,
                      bool hotReload)
    {
        ArgumentNullException.ThrowIfNull(dllPath);
        ArgumentNullException.ThrowIfNull(sharpPath);
        ArgumentNullException.ThrowIfNull(version);
        //ArgumentNullException.ThrowIfNull(coreConfiguration);

        _sharedSystem = sharedSystem ?? throw new ArgumentNullException(nameof(sharedSystem));
        // var configuration = new ConfigurationBuilder().AddJsonFile(Path.Combine(dllPath, "appsettings.json"), false, false).Build();

        var services = new ServiceCollection();

        services.AddSingleton(sharedSystem.GetLoggerFactory());
        services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
        
        // Register external dependencies
        services.AddSingleton(_sharedSystem);
        services.AddSingleton(_sharedSystem.GetEntityManager());
        services.AddSingleton(_sharedSystem.GetEventManager());
        services.AddSingleton(_sharedSystem.GetModSharp());
        services.AddSingleton(_sharedSystem.GetClientManager());

        // Register SqliteDatabase with proper factory
        var path = Path.Combine(sharpPath, "data", "ZombieModSharp.db");
        services.AddSingleton<ISqliteDatabase>(provider => 
            new SqliteDatabase($"Data Source={path}", provider.GetRequiredService<ILogger<SqliteDatabase>>()));
        
        // Register our services using the extension method
        services.AddZombieModSharpServices();

        // _bridge = new InterfaceBridge(dllPath, sharpPath, version, sharedSystem);
        _logger = sharedSystem.GetLoggerFactory().CreateLogger<ZombieModSharp>();
        _serviceProvider = services.BuildServiceProvider();

        // Get services from DI container instead of manual instantiation
        _sqliteDatabase = _serviceProvider.GetRequiredService<ISqliteDatabase>();
        _eventListener = _serviceProvider.GetRequiredService<IEvents>();
        _listeners = _serviceProvider.GetRequiredService<IListeners>();
        _command = _serviceProvider.GetRequiredService<ICommand>();
        _hooks = _serviceProvider.GetRequiredService<IHooks>();
        _configs = _serviceProvider.GetRequiredService<IConfigs>();
        _cvarManager = _serviceProvider.GetRequiredService<ICvarServices>();
    }

    public bool Init()
    {
        _logger.LogInformation(
            "Oh wow, we seem to be crossing paths a lot lately... Where could I have seen you before? Can you figure it out?");

        _listeners.Init();
        _eventListener.Init();

        var _gamedata = _sharedSystem.GetModSharp().GetGameData();
        _gamedata.Register("ZombieModSharp.jsonc");

        var modsharp = _sharedSystem.GetModSharp();
        modsharp.InvokeFrameActionAsync(async () => await _sqliteDatabase.Init());
        _cvarManager.Init();

        _configs.Init();

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
    }

    public void OnAllModulesLoaded()
    {

    }

    public void OnLibraryConnected(string name)
    {

    }

    public void OnLibraryDisconnect(string name)
    {
        
    }
}