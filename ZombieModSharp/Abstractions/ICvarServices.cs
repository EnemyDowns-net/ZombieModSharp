using Sharp.Shared.Objects;

namespace ZombieModSharp.Abstractions;

public interface ICvarServices
{
    public void Init();
    public void Shutdown();
    public Dictionary<string, IConVar?> CvarList { get; set; }
}