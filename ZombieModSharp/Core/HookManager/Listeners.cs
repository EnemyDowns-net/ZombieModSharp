using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;
using ZombieModSharp.Abstractions;
using ZombieModSharp.Abstractions.Storage;

namespace ZombieModSharp.Core.HookManager;

public class Listeners : IListeners, IClientListener, IGameListener
{
    public int ListenerVersion => IClientListener.ApiVersion;
    public int ListenerPriority => 0;

    private readonly IPlayerManager _playerManager;
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<Listeners> _logger;
    private readonly IModSharp _modsharp;
    private readonly ISqliteDatabase _sqlite;
    private readonly ICvarManager _cvar;
    private readonly IPlayerClasses _playerClasses;
    // private readonly IPrecacheManager _precacheManager;
    private readonly IConfigManager _configManager;

    public Listeners(IPlayerManager playerManager, ISharedSystem sharedSystem, ISqliteDatabase sqlite, ICvarManager cvar, IPlayerClasses playerClasses, IConfigManager configManager)
    {
        _playerManager = playerManager;
        _sharedSystem = sharedSystem;
        _logger = _sharedSystem.GetLoggerFactory().CreateLogger<Listeners>();
        _modsharp = _sharedSystem.GetModSharp();
        _sqlite = sqlite;
        _cvar = cvar;
        _playerClasses = playerClasses;
        _configManager = configManager;
    }

    public void Init()
    {
        _sharedSystem.GetClientManager().InstallClientListener(this);
        _modsharp.InstallGameListener(this);
    }

    public void OnClientPutInServer(IGameClient client)
    {
        //_logger.LogInformation("ClientPutInServer: {Name}", client.Name);
        if (client.IsHltv)
            return;

        var id = client.SteamId.ToString();

        string humanClass = string.Empty;
        string zombieClass = string.Empty;

        _modsharp.InvokeFrameActionAsync(async () => {
            var classes = await _sqlite.GetPlayerClassesAsync(id);

            if (classes == null)
            {
                // _logger.LogInformation("Found nothing.");

                humanClass = _cvar.CvarList["Cvar_HumanDefault"]!.GetString();
                zombieClass = _cvar.CvarList["Cvar_ZombieDefault"]!.GetString();

                // _logger.LogInformation("Try insert {human} | {zombie}", humanClass, zombieClass);

                var insertResult = await _sqlite.InsertPlayerClassesAsync(id, humanClass, zombieClass);
                _logger.LogInformation("Insert result for {SteamId}: {Result}", id, insertResult);
            }
            else
            {
                humanClass = classes.HumanClass;
                zombieClass = classes.ZombieClass;

                // _logger.LogInformation("Found {human} | {zombie}", classes.HumanClass, classes.ZombieClass);
            }

            var player = _playerManager.GetOrCreatePlayer(client);

            player.HumanClass = _playerClasses.GetClassByName(humanClass);
            player.ZombieClass = _playerClasses.GetClassByName(zombieClass);
        });
    }

    public void OnClientDisconnecting(IGameClient client, NetworkDisconnectionReason reason)
    {
        //_logger.LogInformation("ClientDisconnect: {Name}", client.Name);
        if (client.IsHltv)
            return;

        _playerManager.RemovePlayer(client);
    }

    public void OnResourcePrecache()
    {
        _logger.LogInformation("Precache GoldShip Here");
        _configManager.PrecacheAllResource();
    }
}