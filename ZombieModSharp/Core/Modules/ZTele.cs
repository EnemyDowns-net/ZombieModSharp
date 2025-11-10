using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Objects;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.Modules;

public class ZTele : IZTele
{
    private readonly ISharedSystem _sharedSystem;
    private readonly IPlayerManager _player;
    private readonly ILogger<ZTele> _logger;
    private readonly IModSharp _modsharp;

    public ZTele(ISharedSystem sharedSystem, IPlayerManager player)
    {
        _sharedSystem = sharedSystem;
        _player = player;
        _logger = _sharedSystem.GetLoggerFactory().CreateLogger<ZTele>();
        _modsharp = _sharedSystem.GetModSharp();
    }

    public void OnPlayerSpawn(IGameClient client)
    {
        var player = _player.GetOrCreatePlayer(client);

        var clientEnt = client.GetPlayerController()?.GetPlayerPawn();

        if (clientEnt == null)
            return;

        if (!clientEnt.IsAlive)
            return;

        player.SpawnPoint = clientEnt.GetAbsOrigin();
        player.SpawnRotation = clientEnt.GetAbsAngles();
    }

    public void TeleportToSpawn(IGameClient client)
    {
        var player = _player.GetOrCreatePlayer(client);

        var clientEnt = client.GetPlayerController()?.GetPlayerPawn();

        if (clientEnt == null)
            return;

        if (!clientEnt.IsAlive)
            return;

        clientEnt.Teleport(player.SpawnPoint, player.SpawnRotation, null);
    }
}