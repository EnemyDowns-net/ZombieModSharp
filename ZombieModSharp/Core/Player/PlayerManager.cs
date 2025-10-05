using Sharp.Shared.Objects;
using ZombieModSharp.Entities;
using ZombieModSharp.Interface.Player;

namespace ZombieModSharp.Core.Player;

public class PlayerManager : IPlayerManager
{
    private Dictionary<IGameClient, ZMPlayer> Players { get; set; } = new();

    public PlayerManager()
    {
        Players = new Dictionary<IGameClient, ZMPlayer>();
    }

    public ZMPlayer GetPlayer(IGameClient client)
    {
        if (Players.ContainsKey(client))
        {
            return Players[client];
        }
        else
        {
            var zmPlayer = new ZMPlayer();
            Players.Add(client, zmPlayer);
            return zmPlayer;
        }
    }

    public Dictionary<IGameClient, ZMPlayer> GetAllPlayers()
    {
        return Players;
    }

    public void RemovePlayer(IGameClient client)
    {
        Players.Remove(client);
    }
}