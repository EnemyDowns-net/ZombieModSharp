using Sharp.Shared.Objects;

namespace ZombieModSharp.Abstractions;

public interface IKnockback
{
    public void KnockbackClient(IGameClient client, IGameClient attacker, string weapon, float damage, int hitGroup);
}