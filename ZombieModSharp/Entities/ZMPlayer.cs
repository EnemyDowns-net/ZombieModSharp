using ZombieModSharp.Enums;

namespace ZombieModSharp.Entities;

public class ZMPlayer
{
    public ZMPlayer()
    {
        IsInfected = false;
        MotherZombieStatus = MotherZombieStatus.None;
    }

    public bool IsInfected { get; set; } = false;
    public MotherZombieStatus MotherZombieStatus { get; set; } = MotherZombieStatus.None;
}