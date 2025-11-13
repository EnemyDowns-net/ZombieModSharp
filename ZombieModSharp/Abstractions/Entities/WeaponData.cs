namespace ZombieModSharp.Abstractions.Entities;

public class WeaponData
{
    public required string EntityName { get; set;}
    public float Knockback { get; set; } = 1.0f;
}