using Sharp.Shared.GameEntities;

namespace ZombieModSharp.Abstractions;

public interface ISoundServices
{
    public void ZombieMoan(IPlayerPawn player);
    public void ZombieHurtSound(IPlayerPawn player);
    public void EmitZombieSound(IBaseEntity source, string sound);
}