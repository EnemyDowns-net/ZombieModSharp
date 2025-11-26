using Sharp.Shared.GameEntities;

namespace ZombieModSharp.Abstractions;

public interface IGrenadeEffect
{
    public bool IgnitePawn(IPlayerPawn? playerPawn, int damage = 1, float duration = 1);
}