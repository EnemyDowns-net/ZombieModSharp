using Sharp.Shared.GameEntities;
using Sharp.Shared.Objects;

namespace ZombieModSharp.Abstractions;

public interface IRespawnServices
{
    public void InitRespawn(IPlayerController? client);
    public void RespawnClient(IPlayerController client);
    public void SetupRespawnToggler();
    public IBaseEntity? GetRespawnToggler();
}