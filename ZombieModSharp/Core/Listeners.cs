using Microsoft.Extensions.Logging;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using ZombieModSharp.Interface.Listeners;
using ZombieModSharp.Interface.Player;

namespace ZombieModSharp.Core;

public class Listeners : IListeners, IClientListener
{
    public int ListenerVersion => IClientListener.ApiVersion;
    public int ListenerPriority => 0;

    private readonly IPlayer _player;
    private readonly ILogger<Listeners> _logger;

    public Listeners(IPlayer player, ILogger<Listeners> logger)
    {
        _player = player;
        _logger = logger;
    }

    public void OnClientPutInServer(IGameClient client)
    {
        _logger.LogInformation("ClientPutInServer: {Name}", client.Name);
        _player.GetPlayer(client);
    }

    public void OnClientDisconnecting(IGameClient client, NetworkDisconnectionReason reason)
    {
        _logger.LogInformation("ClientDisconnect: {Name}", client.Name);
        _player.RemovePlayer(client);
    }
}