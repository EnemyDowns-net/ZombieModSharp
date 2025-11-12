namespace ZombieModSharp.Abstractions;

public interface IPrecacheManager
{
    public void LoadConfig(string path);
    public void PrecacheAllResource();
}