using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using ZombieModSharp.Interface.Listeners;
using ZombieModSharp.Interface.Player;

namespace ZombieModSharp.Core;

public class Listeners : IListeners, IClientListener, IGameListener
{
    public int ListenerVersion => IClientListener.ApiVersion;
    public int ListenerPriority => 0;

    private readonly IPlayer _player;
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<Listeners> _logger;
    private readonly IModSharp _modsharp;

    public Listeners(IPlayer player, ISharedSystem sharedSystem)
    {
        _player = player;
        _sharedSystem = sharedSystem;
        _logger = _sharedSystem.GetLoggerFactory().CreateLogger<Listeners>();
        _modsharp = _sharedSystem.GetModSharp();
    }

    public void Init()
    {
        _sharedSystem.GetClientManager().InstallClientListener(this);
        _modsharp.InstallGameListener(this);
    }

    public void OnClientPutInServer(IGameClient client)
    {
        //_logger.LogInformation("ClientPutInServer: {Name}", client.Name);
        _player.GetPlayer(client);
    }

    public void OnClientDisconnecting(IGameClient client, NetworkDisconnectionReason reason)
    {
        //_logger.LogInformation("ClientDisconnect: {Name}", client.Name);
        _player.RemovePlayer(client);
    }

    public void OnResourcePrecache()
    {
        _logger.LogInformation("Precache GoldShip Here");
        _modsharp.PrecacheResource("characters/models/oylsister/uma_musume/gold_ship/goldship2.vmdl");
        _modsharp.PrecacheResource("characters/models/s2ze/zombie_frozen/zombie_frozen.vmdl");
    }
}