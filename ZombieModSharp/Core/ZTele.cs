using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using TnmsPluginFoundation;
using ZombieModSharp.Interface.Player;
using ZombieModSharp.Interface.ZTele;

public class ZTele : IZTele
{
    private readonly TnmsPlugin _plugin;
    private readonly IPlayer _player;
    private readonly ILogger<ZTele> _logger;
    private readonly IEntityManager _entityManager;

    public ZTele(IServiceProvider serviceProvider, IPlayer player, ILogger<ZTele> logger)
    {
        _plugin = serviceProvider.GetRequiredService<TnmsPlugin>();
        _logger = logger;
        _player = player;
        _entityManager = _plugin.SharedSystem.GetEntityManager();
    }

    public void OnPlayerSpawn(IGameClient client)
    {
        var player = _player.GetPlayer(client);

        var clientEnt = _entityManager.FindPlayerControllerBySlot(client.Slot);

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

        var clientEnt = _entityManager.FindPlayerControllerBySlot(client.Slot);

        if (clientEnt == null)
            return;

        if (!clientEnt.IsAlive)
            return;

        clientEnt.Teleport(player.SpawnPoint, player.SpawnRotation);
    }
}