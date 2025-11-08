using Sharp.Shared.Objects;

namespace ZombieModSharp.Abstractions;

public interface IZTele
{
    public void OnPlayerSpawn(IGameClient client);
    public void TeleportToSpawn(IGameClient client);
}