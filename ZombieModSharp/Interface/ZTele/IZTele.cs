using Sharp.Shared.Objects;

namespace ZombieModSharp.Interface.ZTele;

public interface IZTele
{
    public void OnPlayerSpawn(IGameClient client);
    public void TeleportToSpawn(IGameClient client);
}