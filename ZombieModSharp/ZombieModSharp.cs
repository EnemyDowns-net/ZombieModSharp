using System.Diagnostics.Tracing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PlayerManager_Shared.Abstractions;
using Sharp.Shared;
using Sharp.Shared.Definition;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using ZombieModSharp.Core;
using ZombieModSharp.Core.Infection;
using ZombieModSharp.Core.Player;
using ZombieModSharp.Interface.Events;
using ZombieModSharp.Interface.Infection;
using ZombieModSharp.Interface.Listeners;
using ZombieModSharp.Interface.Player;

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

    // outside modules
    private IPlayerManager? _playerManager;

    public static List<CommandDefinition> commandDefinition = [];

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
        _eventListener = new Events(_sharedSystem.GetEventManager(), _serviceProvider.GetRequiredService<ILogger<Events>>(), _sharedSystem.GetClientManager(), _player, _sharedSystem.GetEntityManager(), _infect, _sharedSystem.GetModSharp());
        _listeners = new Listeners(_player, _serviceProvider.GetRequiredService<ILogger<Listeners>>());
    }

    public bool Init()
    {
        _logger.LogInformation(
            "Oh wow, we seem to be crossing paths a lot lately... Where could I have seen you before? Can you figure it out?");
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

        var clientManager = _sharedSystem.GetClientManager();
        clientManager.InstallClientListener((IClientListener)_listeners);
    }

    public void OnAllModulesLoaded()
    {
        var wrapper = _sharedSystem.GetSharpModuleManager()
                .GetRequiredSharpModuleInterface<IPlayerManager>(IPlayerManager.Identity);
        _playerManager = wrapper.Instance
                         ?? throw new InvalidOperationException("PlayerManager_Shared 介面不可為 null");

        foreach (var command in commandDefinition)
        {
            _sharedSystem.GetClientManager().InstallCommandCallback("ms_ztele", (client, command) => OnClientUseCommand(client, command, Action.))
        }
    }

    public void OnLibraryConnected(string name)
    {
        _logger.LogInformation("The~ Game~ Is~ On~");
    }

    public void OnLibraryDisconnect(string name)
    {
        if (name == "PlayerManager")
        {
            _logger.LogWarning("PlayerManager get excluded or become invalid");
            _playerManager = null;
        }
    }

    private ECommandAction OnClientUseCommand(IGameClient client, StringCommand command, Action<ISharedSystem, IGamePlayer, IGameClient?> action)
    {
        return
    }

    private ECommandAction HandlePlayerTargets(
            IGameClient? client,
            StringCommand command,
            Action<IGamePlayer> action)
    {
        string selector = "@me";
        if (command.ArgCount > 0 && !string.IsNullOrWhiteSpace(command.ArgString))
            selector = command.ArgString.Trim();

        var allPlayers = _playerManager!.GetPlayers();

        var targets = selector switch
        {
            "@me" when client != null => allPlayers.Where(p => p.Client.Equals(client)).ToArray(),
            "@ct" => allPlayers.Where(p =>
            {
                var controller = _sharedSystem.GetEntityManager().FindPlayerControllerBySlot(p.Client.Slot);
                return controller?.Team == CStrikeTeam.CT;
            }).ToArray(),
            "@t" => allPlayers.Where(p =>
            {
                var controller = _sharedSystem.GetEntityManager().FindPlayerControllerBySlot(p.Client.Slot);
                return controller?.Team == CStrikeTeam.TE;
            }).ToArray(),
            _ => allPlayers.Where(p =>
                !string.IsNullOrWhiteSpace(p.Name) &&
                (p.Name.Equals(selector, StringComparison.OrdinalIgnoreCase) ||
                 p.Name.StartsWith(selector, StringComparison.OrdinalIgnoreCase))
            ).ToArray()
        };

        if (targets.Length == 0)
        {
            if (client != null)
                client.SayChatMessage(false, $"{ChatColor.Red}[ADMCommands]{ChatColor.White} 找不到符合條件的玩家：{selector}");
            else
                Console.WriteLine($"找不到符合條件的玩家：{selector}");
            return ECommandAction.Handled;
        }

        foreach (var target in targets)
        {
            action(target);
        }

        return ECommandAction.Handled;
    }
        
    private bool HasPermission(IGameClient client)
    {
        if (_permission is null) return false;

        var steamId = client.SteamId.ToString();
        var identity = _permission.GetIdentity(steamId);

        return identity.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || identity.Equals("Manager", StringComparison.OrdinalIgnoreCase)
                || identity.Equals("Owner", StringComparison.OrdinalIgnoreCase);
    }
}