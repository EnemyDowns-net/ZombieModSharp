using Microsoft.Extensions.Logging;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using ZombieModSharp.Interface.Player;
using ZombieModSharp.Interface.ZTele;

namespace ZombieModSharp.Core;

public class ZTele : IZTele
{
    private readonly IPlayer _player;
    private readonly ILogger<ZTele> _logger;

    public ZTele(IPlayer player, ILogger<ZTele> logger)
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