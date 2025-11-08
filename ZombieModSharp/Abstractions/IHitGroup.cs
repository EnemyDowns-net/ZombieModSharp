namespace ZombieModSharp.Abstractions;

public interface IHitGroup
{
    public void LoadConfig(string path);
    public float GetHitgroupKnockback(int hitgroup);
}