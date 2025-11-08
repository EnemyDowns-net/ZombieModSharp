namespace ZombieModSharp.Abstractions;

public interface IWeapons
{
    public void LoadConfig(string path);
    public float GetWeaponKnockback(string weaponentity);
}