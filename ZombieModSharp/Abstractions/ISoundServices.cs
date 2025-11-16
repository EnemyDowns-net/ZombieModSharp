using Sharp.Shared.GameEntities;

namespace ZombieModSharp.Abstractions;

public interface ISoundServices
{
    public void EmitZombieSound(IBaseEntity source, string sound);
}