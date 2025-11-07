namespace ZombieModSharp.Interface.Configs;

public interface IWeapons
{
    public void LoadConfig(string path);
    public float GetWeaponKnockback(string weaponentity);
}