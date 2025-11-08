using Sharp.Shared.Objects;
using ZombieModSharp.Core.Modules;

namespace ZombieModSharp.Abstractions;

public interface IPlayerManager
{
    public Player GetPlayer(IGameClient client);
    public Dictionary<IGameClient, Player> GetAllPlayers();
    public void RemovePlayer(IGameClient client);
    public bool IsClientInfected(IGameClient client);
    public bool IsClientHuman(IGameClient client);
}
