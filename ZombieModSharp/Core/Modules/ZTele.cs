using Microsoft.Extensions.Logging;
using Sharp.Shared.Objects;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.Modules;

public class ZTele : IZTele
{
    private readonly IPlayerManager _player;
    private readonly ILogger<ZTele> _logger;

    public ZTele(IPlayerManager player, ILogger<ZTele> logger)
    {
        _logger = logger;
        _player = player;
    }

    public void OnPlayerSpawn(IGameClient client)
    {
        var player = _player.GetPlayer(client);

        var clientEnt = client.GetPlayerController();

        if (clientEnt == null)
            return;

        if (!clientEnt.IsAlive)
            return;

        player.SpawnPoint = clientEnt.GetAbsOrigin();
        player.SpawnRotation = clientEnt.GetAbsAngles();
    }

    public void TeleportToSpawn(IGameClient client)
    {
        var player = _player.GetPlayer(client);

        var clientEnt = client.GetPlayerController();

        if (clientEnt == null)
            return;

        if (!clientEnt.IsAlive)
            return;

        clientEnt.Teleport(player.SpawnPoint, player.SpawnRotation);
    }
}