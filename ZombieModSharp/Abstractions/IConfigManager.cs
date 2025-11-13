using ZombieModSharp.Abstractions.Entities;
using ZombieModSharp.Core.Modules;

namespace ZombieModSharp.Abstractions;

public interface IConfigManager
{
    public void Init();
    public void PrecacheAllResource();
    public Dictionary<string, WeaponData> WeaponConfig { get; set; }
    public float[] HitgroupData { get; set; }
}