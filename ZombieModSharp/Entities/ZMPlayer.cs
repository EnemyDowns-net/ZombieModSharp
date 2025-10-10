using Sharp.Shared.Types;
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
    public Vector? SpawnPoint { get; set; } = null;
    public Vector? SpawnRotation { get; set; } = null;

    public bool IsHuman()
    {
        return !IsZombie;
    }

    public bool IsInfected()
    {
        return IsZombie;
    }
}