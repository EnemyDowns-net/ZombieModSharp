using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;
using TnmsPluginFoundation;
using ZombieModSharp.Interface.Player;

namespace ZombieModSharp.Core;

public class Listeners : IClientListener
{
    private readonly TnmsPlugin _plugin;
    private readonly IPlayer _player;
    private readonly ILogger<Listeners> _logger;

    public Listeners(IServiceProvider provider, IPlayer player, ILogger<Listeners> logger)
    {
        _plugin = provider.GetRequiredService<TnmsPlugin>();
        _player = player;
        _logger = logger;
    }

    public int ListenerVersion => IClientListener.ApiVersion;
    public int ListenerPriority => 0;

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