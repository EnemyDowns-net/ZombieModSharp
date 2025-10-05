using Sharp.Shared.Objects;
using ZombieModSharp.Entities;

namespace ZombieModSharp.Interface.Player;

public interface IPlayer
{
    public ZMPlayer GetPlayer(IGameClient client);
    public Dictionary<IGameClient, ZMPlayer> GetAllPlayers();
}