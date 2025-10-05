using ZombieModSharp.Enums;

namespace ZombieModSharp.Entities;

public class ZMPlayer
{
    public ZMPlayer()
    {
        IsZombie = false;
        MotherZombieStatus = MotherZombieStatus.None;
    }

    public bool IsZombie { get; set; } = false;
    public MotherZombieStatus MotherZombieStatus { get; set; } = MotherZombieStatus.None;

    public bool IsHuman()
    {
        return !IsZombie;
    }

    public bool IsInfected()
    {
        return IsZombie;
    }
}