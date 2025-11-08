using Sharp.Shared.GameEntities;
using ZombieModSharp.Core.Modules;

namespace ZombieModSharp.Abstractions;

public interface IPlayerClasses
{
    public void LoadConfig(string path);
    public void ApplyPlayerClassAttribute(IPlayerPawn playerPawn, ClassAttribute classAttribute);
}