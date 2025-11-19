using Sharp.Shared.GameEntities;
using Sharp.Shared.Objects;

namespace ZombieModSharp.Abstractions;

public interface IRespawnServices
{
    public void OnPlayerDeath(IGameClient client);
    public void RespawnClient(IPlayerPawn? playerPawn);
    public bool IsRespawnEnabled();
    public void SetRespawnEnable(bool set = true);
}