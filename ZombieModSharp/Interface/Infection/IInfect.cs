using Sharp.Shared.Objects;

namespace ZombieModSharp.Interface.Infection;

public interface IInfect
{
    public void InfectPlayer(IGameClient client, IGameClient? attacker = null, bool motherzombie = false, bool force = false);
    public void HumanizeClient(IGameClient client, bool force = false);
    public void OnRoundPreStart();
    public void OnRoundStart();
    public void OnRoundEnd();
    public void CheckGameStatus();
    public bool IsInfectStarted();
}