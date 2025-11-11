using Sharp.Shared.Objects;

namespace ZombieModSharp.Abstractions;

public interface ICvarManager
{
    public void Init();
    public Dictionary<string, IConVar?> CvarList { get; set; }
}