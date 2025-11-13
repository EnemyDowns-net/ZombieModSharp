using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using ZombieModSharp.Abstractions;
using ZombieModSharp.Abstractions.Entities;

namespace ZombieModSharp.Core.Modules;

public class PlayerManager : IPlayerManager
{
    private Dictionary<IGameClient, Player> _players { get; set; } = new();

    public PlayerManager()
    {
        _players = new Dictionary<IGameClient, Player>();
    }

    public Player GetOrCreatePlayer(IGameClient client)
    {
        if (_players.ContainsKey(client))
        {
            return _players[client];
        }

        var player = new Player();
        _players.Add(client, player);
        return player;
    }

    public Dictionary<IGameClient, Player> GetAllPlayers()
    {
        return _players;
    }

    public void OnPlayerSpawn(IGameClient client)
    {
        var player = GetOrCreatePlayer(client);

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
        var player = GetOrCreatePlayer(client);

        var clientEnt = client.GetPlayerController()?.GetPlayerPawn();

        if (clientEnt == null)
            return;

        if (!clientEnt.IsAlive)
            return;

        clientEnt.Teleport(player.SpawnPoint, player.SpawnRotation, null);
    }

    public void RemovePlayer(IGameClient client)
    {
        _players.Remove(client);
    }

    public bool IsClientInfected(IGameClient client)
    {
        return _players[client].IsZombie;
    }

    public bool IsClientHuman(IGameClient client)
    {
        return !_players[client].IsZombie;
    }

    public void GetPlayerClassesData(IGameClient client)
    {
        var player = GetOrCreatePlayer(client);
    }
}

// Enhanced ZMPlayer class with all the merged functionality
public class Player
{
    public Player()
    {
        IsZombie = false;
        MotherZombieStatus = MotherZombieStatus.None;

    }

    // Core zombie state
    public bool IsZombie { get; set; } = false;
    public MotherZombieStatus MotherZombieStatus { get; set; } = MotherZombieStatus.None;
    
    // Spawn information
    public Vector? SpawnPoint { get; set; } = null;
    public Vector? SpawnRotation { get; set; } = null;

    // player classes
    public ClassAttribute? HumanClass { get; set; }
    public ClassAttribute? ZombieClass { get; set; }
    public ClassAttribute? ActiveClass { get; set; }

    // Convenience methods
    public bool IsHuman() => !IsZombie;
    public bool IsInfected() => IsZombie;
}