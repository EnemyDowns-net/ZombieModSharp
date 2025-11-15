using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Objects;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.Modules;

public class Knockback : IKnockback
{
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<Knockback> _logger;
    private readonly IPlayerManager _player;
    private readonly IWeapons _weapons;
    private readonly IHitGroup _hitgroup;
    private readonly IModSharp _modsharp;

    public Knockback(ISharedSystem sharedSystem, ILogger<Knockback> logger, IPlayerManager player, IWeapons weapons, IHitGroup hitGroup)
    {
        _sharedSystem = sharedSystem;
        _logger = logger;
        _player = player;
        _weapons = weapons;
        _hitgroup = hitGroup;
        _modsharp = _sharedSystem.GetModSharp();
    }

    public void KnockbackClient(IGameClient client, IGameClient attacker, string weapon, float damage, int hitGroup)
    {
        if (weapon.Contains("hegrenade"))
            return;

        if (client == null || attacker == null)
            return;

        var clientPlayer = _player.GetOrCreatePlayer(client);
        var attackerPlayer = _player.GetOrCreatePlayer(attacker);
            
        // knockback is for zombie only.
        if (!attackerPlayer.IsHuman() || !clientPlayer.IsInfected())
            return;

        var attackerPawn = attacker.GetPlayerController()?.GetPlayerPawn();

        if (attackerPawn == null)
        {
            _logger?.LogError("attacker pawn is null!");
            return;
        }

        var attackerEye = attackerPawn.GetEyeAngles();
        var foward = attackerEye.AnglesToVectorForward();

        var classKnockback = _player.GetOrCreatePlayer(client).ActiveClass?.Knockback ?? 3.0f;
        var weaponknockback = _weapons.GetWeaponKnockback(weapon);
        var hitgroupsKnockback = _hitgroup.GetHitgroupKnockback(hitGroup);

        // _modsharp.PrintToChatAll($"KB data: {weaponknockback:F2} | {hitgroupsKnockback:F2} | {classKnockback:F2}");

        var pushVelocity = foward * damage * classKnockback * weaponknockback * hitgroupsKnockback;
        // _modsharp.PrintToChatAll($"Push Velocity: {pushVelocity}");

        var playerPawn = client.GetPlayerController()?.GetPlayerPawn();

        if (playerPawn == null)
            return;

        var veloCity = playerPawn.GetAbsVelocity();
        playerPawn.Teleport(null, null, veloCity + pushVelocity);
    }
}