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

    public Listeners(IPlayerManager playerManager, ISharedSystem sharedSystem, ISqliteDatabase sqlite)
    {
        _playerManager = playerManager;
        _sharedSystem = sharedSystem;
        _logger = _sharedSystem.GetLoggerFactory().CreateLogger<Listeners>();
        _modsharp = _sharedSystem.GetModSharp();
        _sqlite = sqlite;
    }

    public void Init()
    {
        _sharedSystem.GetClientManager().InstallClientListener(this);
        _modsharp.InstallGameListener(this);
    }

    public void OnClientPutInServer(IGameClient client)
    {
        //_logger.LogInformation("ClientPutInServer: {Name}", client.Name);

        var id = client.SteamId.ToString();

        _modsharp.InvokeFrameActionAsync(async () => {
            var classes = await _sqlite.GetPlayerClassesAsync(id);

            if (classes == null)
            {
                _logger.LogInformation("Found nothing.");
                await _sqlite.InsertPlayerClassesAsync(id, "human_default", "zombie_default");
            }
            else
                _logger.LogInformation("Found {human} | {zombie}", classes.HumanClass, classes.ZombieClass);
        });

        var player = _playerManager.GetOrCreatePlayer(client);
    }

    public void OnClientDisconnecting(IGameClient client, NetworkDisconnectionReason reason)
    {
        //_logger.LogInformation("ClientDisconnect: {Name}", client.Name);
        _playerManager.RemovePlayer(client);
    }

    public void OnResourcePrecache()
    {
        _logger.LogInformation("Precache GoldShip Here");
        _modsharp.PrecacheResource("characters/models/oylsister/uma_musume/gold_ship/goldship2.vmdl");
        _modsharp.PrecacheResource("characters/models/s2ze/zombie_frozen/zombie_frozen.vmdl");
    }
}